using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WUMT.WinVer;

namespace WUMT
{
    public class Backup
    {
        private class BackupFolder
        {
            public DirectoryInfo Source;
            public string Target; // Relative to the backup folder.
        }

        private CommonWinData userData = new CommonWinData();

        public string Destination;
        public string[] Users = new string[0];
        public string[] ExtraFolders = new string[0];

        public bool Desktop = false;
        public bool Personal = false;
        public bool NetworkDrives = false;
        public bool Printers = false;
        public bool RecursiveCopy = true;

        private string destFolder;
        private long totalSize = 0;
        private long copiedSize = 0;
        private volatile bool isDone = false;
        private volatile bool cancelRequested = false;
        private List<BackupFolder> folders = new List<BackupFolder>();
        private List<string> warnings = new List<string>();
        private List<string> errors = new List<string>();

        public Backup()
        {
            destFolder = DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss");
        }

        public string BackupPath
        {
            get { return Path.Combine(Destination, destFolder); }
        }

        public int CopiedPercentage
        {
            get
            {
                long total = Interlocked.Read(ref totalSize);
                if (total <= 0)
                    return 0;

                long percentage = Interlocked.Read(ref copiedSize) * 100 / total;
                return (int)Math.Min(100, Math.Max(0, percentage));
            }
        }

        public long CopiedSize
        {
            get { return Interlocked.Read(ref copiedSize); }
        }

        public bool IsDone
        {
            get { return isDone; }
        }

        public bool Cancelled
        {
            get { return cancelRequested; }
        }

        /// <summary>
        /// Problems found while preparing the folder list (missing folders, skipped ones...).
        /// </summary>
        public string[] Warnings
        {
            get { lock (warnings) return warnings.ToArray(); }
        }

        /// <summary>
        /// Files or folders that could not be read or copied.
        /// </summary>
        public string[] Errors
        {
            get { lock (errors) return errors.ToArray(); }
        }

        public void Cancel()
        {
            cancelRequested = true;
        }

        public string[] AllFolders()
        {
            string[] result = new string[folders.Count];
            for (int i = 0; i < folders.Count; i++)
                result[i] = folders[i].Source.FullName;

            return result;
        }

        /// <summary>
        /// Builds the list of folders to copy from the selected users and extra folders.
        /// Call it after setting all the options.
        /// </summary>
        public void PrepareFolders()
        {
            folders.Clear();
            lock (warnings) warnings.Clear();

            foreach (string user in Users)
            {
                if (Desktop)
                    AddUserFolder(user, "Desktop");
                if (Personal)
                    AddUserFolder(user, "Personal");
            }

            foreach (string folder in ExtraFolders)
            {
                AddFolder(folder, GetTargetFromPath(folder));
            }
        }

        private void AddUserFolder(string user, string shellFolder)
        {
            string path = userData.GetUserPath(user, shellFolder);
            if (String.IsNullOrEmpty(path))
            {
                AddWarning("Could not find the " + shellFolder + " folder of user " + user + ".");
                return;
            }

            string name = shellFolder == "Personal" ? "Documents" : shellFolder;
            AddFolder(path, Path.Combine(Path.Combine("Users", user), name));
        }

        private void AddFolder(string path, string target)
        {
            DirectoryInfo dir;
            try
            {
                dir = new DirectoryInfo(path);
            }
            catch (Exception ex)
            {
                AddWarning("Invalid folder " + path + ": " + ex.Message);
                return;
            }

            if (!dir.Exists)
            {
                AddWarning("Folder not found: " + dir.FullName);
                return;
            }

            foreach (BackupFolder f in folders)
            {
                if (String.Equals(f.Source.FullName.TrimEnd('\\'), dir.FullName.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    return;
            }

            // Copying a folder that contains the destination would copy the backup into itself.
            if (Destination != null && IsSameOrInside(Path.GetFullPath(Destination), dir.FullName, RecursiveCopy))
            {
                AddWarning("Skipped " + dir.FullName + " because it contains the backup destination.");
                return;
            }

            BackupFolder bf = new BackupFolder();
            bf.Source = dir;
            bf.Target = target;
            folders.Add(bf);
        }

        private static bool IsSameOrInside(string path, string folder, bool includeSubfolders)
        {
            string p = path.TrimEnd('\\') + @"\";
            string f = folder.TrimEnd('\\') + @"\";

            if (String.Equals(p, f, StringComparison.OrdinalIgnoreCase))
                return true;

            return includeSubfolders && p.StartsWith(f, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// "C:\Data" goes to "C\Data" and "\\server\share\Data" to "UNC\server\share\Data",
        /// so folders with the same name on different drives don't overwrite each other.
        /// </summary>
        private static string GetTargetFromPath(string path)
        {
            string full;
            try
            {
                full = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                full = path;
            }

            if (full.StartsWith(@"\\"))
                return "UNC" + @"\" + full.Substring(2).TrimEnd('\\');

            string root = Path.GetPathRoot(full);
            string drive = root.TrimEnd('\\').TrimEnd(':');
            string rest = full.Substring(root.Length).TrimEnd('\\');

            return rest.Length > 0 ? Path.Combine(drive, rest) : drive;
        }

        public long GetTotalSize()
        {
            long size = 0;

            foreach (BackupFolder folder in folders)
            {
                size += GetDirectorySize(folder.Source, RecursiveCopy, null);
            }

            return size;
        }

        public void StartBackup()
        {
            Directory.CreateDirectory(BackupPath);

            Ini iniFile = new Ini(Path.Combine(BackupPath, "Backup.ini"));
            iniFile.Write("Global", "Destination", Destination);
            iniFile.Write("Global", "UserList", String.Join(",", Users));
            iniFile.Write("Global", "RecursiveCopy", RecursiveCopy.ToString());

            foreach (string user in Users)
            {
                if (Desktop)
                    iniFile.Write(user, "Desktop", userData.GetUserPath(user, "Desktop"));
                if (Personal)
                    iniFile.Write(user, "Personal", userData.GetUserPath(user, "Personal"));
            }

            int i;

            // Mapped drives can only be read for the user running the application.
            if (NetworkDrives)
            {
                try
                {
                    i = 0;
                    foreach (KeyValuePair<string, string> nDrv in userData.GetMappedNetworkDrives())
                    {
                        iniFile.Write("NetworkDrives", "NetworkDrive-" + i, nDrv.Key + "||" + nDrv.Value);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    AddError("Could not read the network drives: " + ex.Message);
                }
            }

            if (Printers)
            {
                try
                {
                    i = 0;
                    foreach (KeyValuePair<string, string[]> nPrns in userData.GetMappedPrinters())
                    {
                        foreach (string nPr in nPrns.Value)
                        {
                            iniFile.Write("Printers", "Printer-" + i, nPrns.Key + "||" + nPr);
                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddError("Could not read the printers: " + ex.Message);
                }
            }

            for (int j = 0; j < folders.Count; j++)
                iniFile.Write("All", j.ToString(), folders[j].Source.FullName + "||" + folders[j].Target);

            Interlocked.Exchange(ref totalSize, GetTotalSize());

            StartCopy();

            string[] errorList = Errors;
            if (errorList.Length > 0)
            {
                try
                {
                    File.WriteAllLines(Path.Combine(BackupPath, "Errors.log"), errorList);
                }
                catch (Exception) { }
            }
        }

        public void StartCopy()
        {
            foreach (BackupFolder folder in folders)
            {
                if (cancelRequested)
                    break;

                CopyDirectory(folder.Source, Path.Combine(BackupPath, folder.Target), RecursiveCopy);
            }

            isDone = true;
        }

        private void CopyDirectory(DirectoryInfo source, string destDirectory, bool recursive)
        {
            FileInfo[] files;
            try
            {
                Directory.CreateDirectory(destDirectory);
                files = source.GetFiles();
            }
            catch (Exception ex)
            {
                AddError(source.FullName + ": " + ex.Message);
                return;
            }

            foreach (FileInfo file in files)
            {
                if (cancelRequested)
                    return;

                try
                {
                    string target = Path.Combine(destDirectory, file.Name);

                    // Overwriting a read-only file fails, so clear the attribute first.
                    if (File.Exists(target))
                        File.SetAttributes(target, FileAttributes.Normal);

                    file.CopyTo(target, true);
                }
                catch (Exception ex)
                {
                    AddError(file.FullName + ": " + ex.Message);
                }

                Interlocked.Add(ref copiedSize, file.Length);
            }

            if (!recursive)
                return;

            foreach (DirectoryInfo directory in GetSubdirectories(source, this))
            {
                if (cancelRequested)
                    return;

                CopyDirectory(directory, Path.Combine(destDirectory, directory.Name), true);
            }
        }

        /// <summary>
        /// Size of the files in a folder. Skips folders that can't be read.
        /// </summary>
        internal static long GetDirectorySize(DirectoryInfo source, bool recursive, Backup owner)
        {
            long size = 0;

            try
            {
                foreach (FileInfo fi in source.GetFiles())
                    size += fi.Length;
            }
            catch (Exception)
            {
                return 0;
            }

            if (recursive)
            {
                foreach (DirectoryInfo di in GetSubdirectories(source, owner))
                    size += GetDirectorySize(di, true, owner);
            }

            return size;
        }

        /// <summary>
        /// Subfolders, without junctions and symbolic links ("My Music" inside Documents, etc.),
        /// which are usually not readable and point to folders that are copied anyway.
        /// </summary>
        private static List<DirectoryInfo> GetSubdirectories(DirectoryInfo source, Backup owner)
        {
            List<DirectoryInfo> result = new List<DirectoryInfo>();

            try
            {
                foreach (DirectoryInfo di in source.GetDirectories())
                {
                    if ((di.Attributes & FileAttributes.ReparsePoint) == 0)
                        result.Add(di);
                }
            }
            catch (Exception ex)
            {
                if (owner != null)
                    owner.AddError(source.FullName + ": " + ex.Message);
            }

            return result;
        }

        private void AddWarning(string message)
        {
            lock (warnings) warnings.Add(message);
        }

        private void AddError(string message)
        {
            lock (errors) errors.Add(message);
        }
    }

    // TODO: restore is not implemented yet.
    public class Restore
    {
    }

    /// <summary>
    /// Extends DirectoryInfo class to add it getting size skills.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Gets the size of files contained in a directory and its subdirectories.
        /// Folders that can't be read are skipped.
        /// </summary>
        public static long DirectorySize(this DirectoryInfo source)
        {
            return Backup.GetDirectorySize(source, true, null);
        }
    }

    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class Ini
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private string path;

        public Ini(string INIPath)
        {
            path = INIPath;
        }

        /// <summary>
        /// Write data to the ini file.
        /// </summary>
        public void Write(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// Read data value from the ini file.
        /// </summary>
        public string Read(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, Key, "", temp, temp.Capacity, this.path);
            return temp.ToString();
        }
    }
}
