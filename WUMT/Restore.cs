using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using WUMT.WinVer;

namespace WUMT
{
    /// <summary>
    /// A user found in a backup or in the disk of another computer.
    /// </summary>
    public class MigrationUser
    {
        public string Name;
        public string DesktopSource;   // Null if not available.
        public string PersonalSource;  // Null if not available.
        public Dictionary<string, string> NetworkDrives = new Dictionary<string, string>();
        public List<string> Printers = new List<string>();
    }

    /// <summary>
    /// An extra folder found in a backup or chosen in the disk of another computer.
    /// </summary>
    public class MigrationFolder
    {
        public string Source;        // Where the files are now.
        public string OriginalPath;  // Where they were in the old computer, mapped to this one.

        public override string ToString()
        {
            return OriginalPath;
        }
    }

    /// <summary>
    /// Restores a backup made by WUMT, or copies the data straight from the disk
    /// of another computer connected to this one.
    /// </summary>
    public class Restore : IMigrationJob
    {
        public enum SourceKind { Backup, Disk }

        private static readonly string[] SystemProfiles = new string[] {
            "All Users", "Default", "Default User", "Public", "LocalService", "NetworkService", "defaultuser0"
        };

        private CommonWinData userData = new CommonWinData();
        private FileCopier copier = new FileCopier();
        private List<string> notes = new List<string>();
        private string logFile;

        public SourceKind Kind;
        public string SourcePath;           // Backup folder or disk root.
        public string Description = "";
        public bool Compressed = false;
        public bool RecursiveCopy = true;
        public List<MigrationUser> SourceUsers = new List<MigrationUser>();
        public List<MigrationFolder> ExtraFolders = new List<MigrationFolder>();

        // Options chosen in the wizard.
        public Dictionary<string, string> UserMapping = new Dictionary<string, string>(); // Source user -> local user.
        public List<MigrationFolder> SelectedFolders = new List<MigrationFolder>();
        public string AlternativeRoot = null; // Null to restore extra folders to their original location.
        public bool RestoreDesktop = true;
        public bool RestorePersonal = true;
        public bool RestoreNetworkDrives = true;
        public bool RestorePrinters = true;
        public bool Overwrite = false;

        private class CopyItem
        {
            public string Source;
            public string Target;
        }

        public int CopiedPercentage
        {
            get { return copier.CopiedPercentage; }
        }

        public bool Cancelled
        {
            get { return copier.Cancelled; }
        }

        public string[] Errors
        {
            get { return copier.Errors; }
        }

        /// <summary>
        /// Things the user should know that are not errors (local printers to install, etc.).
        /// </summary>
        public string[] Notes
        {
            get { lock (notes) return notes.ToArray(); }
        }

        public string LogFile
        {
            get { return logFile; }
        }

        public void Cancel()
        {
            copier.Cancel();
        }

        private void AddNote(string note)
        {
            lock (notes) notes.Add(note);
        }

        /* ****************************************************************************** */
        /* *                               SOURCES                                      * */
        /* ****************************************************************************** */

        /// <summary>
        /// Looks for WUMT backups in the root of every drive and one level below.
        /// </summary>
        public static List<string> FindBackups()
        {
            List<string> backups = new List<string>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (!drive.IsReady || (drive.DriveType != DriveType.Removable && drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Network))
                        continue;

                    foreach (string dir in Directory.GetDirectories(drive.RootDirectory.FullName))
                    {
                        if (File.Exists(Path.Combine(dir, Backup.IniName)))
                        {
                            backups.Add(dir);
                            continue;
                        }

                        try
                        {
                            foreach (string sub in Directory.GetDirectories(dir))
                            {
                                if (File.Exists(Path.Combine(sub, Backup.IniName)))
                                    backups.Add(sub);
                            }
                        }
                        catch (Exception)
                        { }
                    }
                }
                catch (Exception)
                { }
            }

            backups.Sort(StringComparer.OrdinalIgnoreCase);
            backups.Reverse(); // Newest first: folder names are dates.
            return backups;
        }

        /// <summary>
        /// Drives, other than the one where this Windows runs, that contain Windows user profiles.
        /// </summary>
        public static List<string> FindWindowsDisks()
        {
            List<string> disks = new List<string>();
            string systemRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (!drive.IsReady || (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable))
                        continue;
                    if (String.Equals(drive.RootDirectory.FullName, systemRoot, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (FindProfileFolders(drive.RootDirectory.FullName).Count > 0)
                        disks.Add(drive.RootDirectory.FullName);
                }
                catch (Exception)
                { }
            }

            return disks;
        }

        private static List<string> FindProfileFolders(string diskRoot)
        {
            List<string> profiles = new List<string>();

            foreach (string container in new string[] { "Users", "Documents and Settings" })
            {
                string path = Path.Combine(diskRoot, container);
                if (!Directory.Exists(path))
                    continue;

                try
                {
                    foreach (string profile in Directory.GetDirectories(path))
                    {
                        string name = Path.GetFileName(profile);
                        if (Array.Exists(SystemProfiles, delegate(string s) { return String.Equals(s, name, StringComparison.OrdinalIgnoreCase); }))
                            continue;

                        if (File.Exists(Path.Combine(profile, "NTUSER.DAT")))
                            profiles.Add(profile);
                    }
                }
                catch (Exception)
                { }
            }

            return profiles;
        }

        /// <summary>
        /// Reads a backup made by WUMT.
        /// </summary>
        public static Restore FromBackup(string backupFolder)
        {
            string iniPath = Path.Combine(backupFolder, Backup.IniName);
            if (!File.Exists(iniPath))
                throw new FileNotFoundException("There is no " + Backup.IniName + " in " + backupFolder + ".");

            Ini ini = new Ini(iniPath);
            Restore restore = new Restore();
            restore.Kind = SourceKind.Backup;
            restore.SourcePath = backupFolder.TrimEnd('\\');
            restore.Compressed = ParseBool(ini.Read("Global", "Compressed"), false);
            restore.RecursiveCopy = ParseBool(ini.Read("Global", "RecursiveCopy"), true);

            string date = ini.Read("Global", "Date");
            string computer = ini.Read("Global", "Computer");
            restore.Description = "Backup " + (date.Length > 0 ? "of " + date : Path.GetFileName(restore.SourcePath))
                + (computer.Length > 0 ? " from " + computer : "")
                + (restore.Compressed ? " (compressed)" : "") + ".";

            foreach (string name in ini.Read("Global", "UserList").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                MigrationUser user = new MigrationUser();
                user.Name = name;

                string desktop = Path.Combine(restore.SourcePath, Backup.GetUserFolderTarget(name, "Desktop"));
                string personal = Path.Combine(restore.SourcePath, Backup.GetUserFolderTarget(name, "Personal"));
                if (LongPath.DirectoryExists(desktop))
                    user.DesktopSource = desktop;
                if (LongPath.DirectoryExists(personal))
                    user.PersonalSource = personal;

                foreach (string key in ini.GetKeys(name))
                {
                    string value = ini.Read(name, key);
                    if (key.StartsWith("NetworkDrive-"))
                    {
                        string[] parts = value.Split(new string[] { "||" }, StringSplitOptions.None);
                        if (parts.Length == 2)
                            user.NetworkDrives[parts[0]] = parts[1];
                    }
                    else if (key.StartsWith("Printer-") && value.StartsWith(@"\\"))
                    {
                        user.Printers.Add(value);
                    }
                }

                restore.SourceUsers.Add(user);
            }

            // Version 1 backups don't have the [Extra] section: take [All] without the user folders.
            bool hasExtra = ini.GetKeys("Extra").Length > 0;
            string section = hasExtra ? "Extra" : "All";
            foreach (string key in ini.GetKeys(section))
            {
                string[] parts = ini.Read(section, key).Split(new string[] { "||" }, StringSplitOptions.None);
                if (parts.Length != 2 || (!hasExtra && parts[1].StartsWith(@"Users\", StringComparison.OrdinalIgnoreCase)))
                    continue;

                MigrationFolder folder = new MigrationFolder();
                folder.OriginalPath = parts[0];
                folder.Source = Path.Combine(restore.SourcePath, parts[1]);
                if (LongPath.DirectoryExists(folder.Source))
                    restore.ExtraFolders.Add(folder);
            }

            // Local printers need their drivers, so they can only be listed.
            foreach (string key in ini.GetKeys("Printers"))
            {
                string value = ini.Read("Printers", key);
                if (value.StartsWith("Local||"))
                    restore.AddNote("Local printer \"" + value.Substring(7) + "\" must be installed manually with its driver.");
            }

            return restore;
        }

        /// <summary>
        /// Reads the users of the disk of another computer connected to this one.
        /// With administrator rights, the old registry of each user is read too
        /// (moved folders, network drives and printers).
        /// </summary>
        public static Restore FromDisk(string diskRoot)
        {
            Restore restore = new Restore();
            restore.Kind = SourceKind.Disk;
            restore.SourcePath = diskRoot;
            restore.Description = "Windows disk " + diskRoot.TrimEnd('\\') + ".";

            foreach (string profile in FindProfileFolders(diskRoot))
            {
                MigrationUser user = new MigrationUser();
                user.Name = Path.GetFileName(profile);

                string desktop = null, personal = null;

                try
                {
                    using (UserHive hive = UserHive.Load(Path.Combine(profile, "NTUSER.DAT")))
                    {
                        if (hive != null)
                        {
                            desktop = MapOldPath(hive.GetShellFolder("Desktop"), profile, diskRoot);
                            personal = MapOldPath(hive.GetShellFolder("Personal"), profile, diskRoot);
                            user.NetworkDrives = hive.GetNetworkDrives();
                            user.Printers = hive.GetPrinterConnections();
                        }
                    }
                }
                catch (Exception)
                { }

                user.DesktopSource = desktop ?? CommonWinData.GetDefaultUserPath(profile, "Desktop");
                user.PersonalSource = personal ?? CommonWinData.GetDefaultUserPath(profile, "Personal");

                restore.SourceUsers.Add(user);
            }

            return restore;
        }

        /// <summary>
        /// Translates a path of the old computer to the connected disk.
        /// </summary>
        private static string MapOldPath(string value, string profile, string diskRoot)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            string path;
            if (value.StartsWith("%USERPROFILE%", StringComparison.OrdinalIgnoreCase))
                path = profile + value.Substring("%USERPROFILE%".Length);
            else if (value.Length > 2 && value[1] == ':')
                path = Path.Combine(diskRoot, value.Substring(3)); // Assumes it was in the old system drive.
            else
                return null;

            return Directory.Exists(path) ? path : null;
        }

        /// <summary>
        /// Adds a folder of the connected disk to copy (disk mode).
        /// It goes to the same place in this computer's system drive unless AlternativeRoot is set.
        /// </summary>
        public MigrationFolder AddDiskFolder(string path)
        {
            string full = Path.GetFullPath(path).TrimEnd('\\');
            string root = Path.GetPathRoot(full);
            string systemRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

            MigrationFolder folder = new MigrationFolder();
            folder.Source = full;
            folder.OriginalPath = Path.Combine(systemRoot, full.Substring(root.Length));
            ExtraFolders.Add(folder);
            return folder;
        }

        private static bool ParseBool(string value, bool defaultValue)
        {
            bool result;
            return Boolean.TryParse(value, out result) ? result : defaultValue;
        }

        /* ****************************************************************************** */
        /* *                                 PLAN                                       * */
        /* ****************************************************************************** */

        private List<CopyItem> BuildCopyList(List<string> problems)
        {
            List<CopyItem> items = new List<CopyItem>();

            foreach (MigrationUser user in SourceUsers)
            {
                string local;
                if (!UserMapping.TryGetValue(user.Name, out local))
                    continue;

                if (RestoreDesktop && user.DesktopSource != null)
                    AddCopyItem(items, problems, user.DesktopSource, userData.GetUserPath(local, "Desktop"), "Desktop of " + local);
                if (RestorePersonal && user.PersonalSource != null)
                    AddCopyItem(items, problems, user.PersonalSource, userData.GetUserPath(local, "Personal"), "Documents of " + local);
            }

            foreach (MigrationFolder folder in SelectedFolders)
            {
                string target = AlternativeRoot == null
                    ? folder.OriginalPath
                    : Path.Combine(AlternativeRoot, Backup.GetTargetFromPath(folder.OriginalPath));
                AddCopyItem(items, problems, folder.Source, target, folder.OriginalPath);
            }

            return items;
        }

        private void AddCopyItem(List<CopyItem> items, List<string> problems, string source, string target, string description)
        {
            if (String.IsNullOrEmpty(target))
            {
                problems.Add("Could not find the destination folder for " + description + ".");
                return;
            }

            if (Backup.IsSameOrInside(target, source, true) || Backup.IsSameOrInside(source, target, true))
            {
                problems.Add("Skipped " + source + " because it overlaps with " + target + ".");
                return;
            }

            CopyItem item = new CopyItem();
            item.Source = source;
            item.Target = target;
            items.Add(item);
        }

        /// <summary>
        /// Text describing what will be done.
        /// </summary>
        public List<string> GetPlan()
        {
            List<string> problems = new List<string>();
            List<CopyItem> items = BuildCopyList(problems);
            List<string> lines = new List<string>();

            foreach (string problem in problems)
                lines.Add("Warning: " + problem);

            if (items.Count > 0)
            {
                lines.Add("These folders will be copied" + (Overwrite ? " (overwriting existing files):" : " (keeping existing files):"));
                foreach (CopyItem item in items)
                    lines.Add("  " + item.Source + "  ->  " + item.Target);
            }
            else
            {
                lines.Add("No folders will be copied.");
            }

            foreach (MigrationUser user in SourceUsers)
            {
                string local;
                if (!UserMapping.TryGetValue(user.Name, out local))
                    continue;

                if (RestoreNetworkDrives)
                    foreach (KeyValuePair<string, string> drive in user.NetworkDrives)
                        lines.Add("Network drive " + drive.Key + " " + drive.Value + " for " + local + ".");

                if (RestorePrinters)
                    foreach (string printer in user.Printers)
                        lines.Add("Network printer " + printer + " for " + local + ".");
            }

            foreach (string note in Notes)
                lines.Add("Note: " + note);

            long size = 0;
            foreach (CopyItem item in items)
                size += FileCopier.Measure(item.Source, RecursiveCopy);
            lines.Add("Total size: " + String.Format("{0:#,0.0}", size / 1024f / 1024f) + "MB.");

            return lines;
        }

        /* ****************************************************************************** */
        /* *                                  RUN                                       * */
        /* ****************************************************************************** */

        public void Run()
        {
            List<string> problems = new List<string>();
            List<CopyItem> items = BuildCopyList(problems);
            foreach (string problem in problems)
                copier.AddError(problem);

            copier.Decompress = Compressed;
            copier.Overwrite = Overwrite;

            long total = 0;
            foreach (CopyItem item in items)
                total += FileCopier.Measure(item.Source, RecursiveCopy);
            copier.TotalSize = total;

            foreach (CopyItem item in items)
            {
                if (copier.Cancelled)
                    break;
                copier.CopyDirectory(item.Source, item.Target, RecursiveCopy);
            }

            if (!copier.Cancelled)
            {
                foreach (MigrationUser user in SourceUsers)
                {
                    string local;
                    if (!UserMapping.TryGetValue(user.Name, out local))
                        continue;

                    if (RestoreNetworkDrives)
                        foreach (KeyValuePair<string, string> drive in user.NetworkDrives)
                            RestoreNetworkDrive(local, drive.Key, drive.Value);

                    if (RestorePrinters)
                        foreach (string printer in user.Printers)
                            RestorePrinter(local, printer);
                }
            }

            WriteLog();
        }

        private bool IsCurrentUser(string local)
        {
            return String.Equals(local, userData.GetCurrentUserName(), StringComparison.OrdinalIgnoreCase);
        }

        private void RestoreNetworkDrive(string local, string letter, string remotePath)
        {
            letter = letter.TrimEnd('\\').TrimEnd(':').ToUpper() + ":";

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.Name.StartsWith(letter, StringComparison.OrdinalIgnoreCase) && drive.DriveType != DriveType.Network)
                {
                    copier.AddError("Network drive " + letter + " (" + remotePath + ") not restored: the letter is used by a local drive.");
                    return;
                }
            }

            // For the current user, connect it now and make it persistent.
            if (IsCurrentUser(local) && NetworkConnection.Map(letter, remotePath) == 0)
                return;

            // Otherwise (other user or server not available now), Windows will connect it at logon.
            try
            {
                using (UserHive hive = userData.OpenUserHive(local))
                {
                    if (hive == null)
                    {
                        copier.AddError("Network drive " + letter + " not restored for " + local + ": run WUMT as administrator.");
                        return;
                    }
                    hive.AddNetworkDrive(letter, remotePath);
                }
            }
            catch (Exception ex)
            {
                copier.AddError("Network drive " + letter + " not restored for " + local + ": " + ex.Message);
            }
        }

        private void RestorePrinter(string local, string printer)
        {
            if (IsCurrentUser(local))
            {
                try
                {
                    using (ManagementClass printers = new ManagementClass("Win32_Printer"))
                    {
                        object result = printers.InvokeMethod("AddPrinterConnection", new object[] { printer });
                        if (Convert.ToUInt32(result) == 0)
                            return;
                    }
                }
                catch (Exception)
                { }
            }

            // Other users, or printer not available now: Windows will connect it at logon.
            try
            {
                using (UserHive hive = userData.OpenUserHive(local))
                {
                    if (hive == null)
                    {
                        copier.AddError("Printer " + printer + " not restored for " + local + ": run WUMT as administrator.");
                        return;
                    }
                    hive.AddPrinterConnection(printer);
                }
            }
            catch (Exception ex)
            {
                copier.AddError("Printer " + printer + " not restored for " + local + ": " + ex.Message);
            }
        }

        private void WriteLog()
        {
            string[] errorList = Errors;
            if (errorList.Length == 0)
                return;

            string name = "Restore-" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss") + ".log";
            List<string> folders = new List<string>();
            if (Kind == SourceKind.Backup)
                folders.Add(SourcePath);
            folders.Add(Path.GetTempPath());

            foreach (string folder in folders)
            {
                try
                {
                    string path = Path.Combine(folder, name);
                    File.WriteAllLines(path, errorList);
                    logFile = path;
                    return;
                }
                catch (Exception)
                { }
            }
        }
    }

    /// <summary>
    /// Connects network drives with the Windows API.
    /// </summary>
    internal static class NetworkConnection
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 1; // RESOURCETYPE_DISK
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        private const int CONNECT_UPDATE_PROFILE = 0x1;

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(NETRESOURCE lpNetResource, string lpPassword, string lpUserName, int dwFlags);

        /// <summary>
        /// Maps a persistent network drive for the current user. Returns 0 on success.
        /// </summary>
        public static int Map(string letter, string remotePath)
        {
            NETRESOURCE resource = new NETRESOURCE();
            resource.lpLocalName = letter;
            resource.lpRemoteName = remotePath;
            return WNetAddConnection2(resource, null, null, CONNECT_UPDATE_PROFILE);
        }
    }
}
