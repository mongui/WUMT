using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using WUMT.WinVer;

namespace WUMT
{
    /// <summary>
    /// A long task (backup, restore or copy) run by the wizard in the background.
    /// </summary>
    public interface IMigrationJob
    {
        void Run();
        void Cancel();
        bool Cancelled { get; }
        int CopiedPercentage { get; }
        string[] Errors { get; }
        /// <summary>Where the error log was saved, or null.</summary>
        string LogFile { get; }
    }

    public class Backup : IMigrationJob
    {
        public const string IniName = "Backup.ini";

        private class BackupFolder
        {
            public string Source;
            public string Target; // Relative to the backup folder.
            public string User;   // Null for extra folders.
            public string Kind;   // "Desktop", "Personal" or null.
        }

        private CommonWinData userData = new CommonWinData();
        private FileCopier copier = new FileCopier();

        public string Destination;
        public string[] Users = new string[0];
        public string[] ExtraFolders = new string[0];

        public bool Desktop = false;
        public bool Personal = false;
        public bool NetworkDrives = false;
        public bool Printers = false;
        public bool RecursiveCopy = true;
        public bool Compress = false;

        private string destFolder;
        private string logFile;
        private volatile bool isDone = false;
        private List<BackupFolder> folders = new List<BackupFolder>();
        private List<string> warnings = new List<string>();

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
            get { return copier.CopiedPercentage; }
        }

        public long CopiedSize
        {
            get { return copier.CopiedSize; }
        }

        public bool IsDone
        {
            get { return isDone; }
        }

        public bool Cancelled
        {
            get { return copier.Cancelled; }
        }

        public string LogFile
        {
            get { return logFile; }
        }

        /// <summary>
        /// Problems found while preparing the folder list (missing folders, skipped ones...).
        /// </summary>
        public string[] Warnings
        {
            get { return warnings.ToArray(); }
        }

        /// <summary>
        /// Files or folders that could not be read or copied.
        /// </summary>
        public string[] Errors
        {
            get { return copier.Errors; }
        }

        public void Cancel()
        {
            copier.Cancel();
        }

        public string[] AllFolders()
        {
            string[] result = new string[folders.Count];
            for (int i = 0; i < folders.Count; i++)
                result[i] = folders[i].Source;

            return result;
        }

        /// <summary>
        /// Builds the list of folders to copy from the selected users and extra folders.
        /// Call it after setting all the options.
        /// </summary>
        public void PrepareFolders()
        {
            folders.Clear();
            warnings.Clear();

            foreach (string user in Users)
            {
                if (Desktop)
                    AddUserFolder(user, "Desktop");
                if (Personal)
                    AddUserFolder(user, "Personal");
            }

            foreach (string folder in ExtraFolders)
            {
                AddFolder(folder, GetTargetFromPath(folder), null, null);
            }
        }

        public static string GetUserFolderTarget(string user, string shellFolder)
        {
            string name = shellFolder == "Personal" ? "Documents" : shellFolder;
            return Path.Combine(Path.Combine("Users", user), name);
        }

        private void AddUserFolder(string user, string shellFolder)
        {
            string path = userData.GetUserPath(user, shellFolder);
            if (String.IsNullOrEmpty(path))
            {
                warnings.Add("Could not find the " + shellFolder + " folder of user " + user + ".");
                return;
            }

            AddFolder(path, GetUserFolderTarget(user, shellFolder), user, shellFolder);
        }

        private void AddFolder(string path, string target, string user, string kind)
        {
            string full;
            try
            {
                full = Path.GetFullPath(path).TrimEnd('\\');
                if (full.EndsWith(":"))
                    full += @"\";
            }
            catch (Exception ex)
            {
                warnings.Add("Invalid folder " + path + ": " + ex.Message);
                return;
            }

            if (!LongPath.DirectoryExists(full))
            {
                warnings.Add("Folder not found: " + full);
                return;
            }

            foreach (BackupFolder f in folders)
            {
                if (String.Equals(f.Source.TrimEnd('\\'), full.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    return;
            }

            // Copying a folder that contains the destination would copy the backup into itself.
            if (Destination != null && IsSameOrInside(Path.GetFullPath(Destination), full, RecursiveCopy))
            {
                warnings.Add("Skipped " + full + " because it contains the backup destination.");
                return;
            }

            BackupFolder bf = new BackupFolder();
            bf.Source = full;
            bf.Target = target;
            bf.User = user;
            bf.Kind = kind;
            folders.Add(bf);
        }

        internal static bool IsSameOrInside(string path, string folder, bool includeSubfolders)
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
        public static string GetTargetFromPath(string path)
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
                size += FileCopier.Measure(folder.Source, RecursiveCopy);

            return size;
        }

        public void Run()
        {
            StartBackup();
        }

        public void StartBackup()
        {
            Directory.CreateDirectory(BackupPath);

            // A file that starts with a UTF-16 BOM makes Windows write the INI in Unicode.
            string iniPath = Path.Combine(BackupPath, IniName);
            File.WriteAllBytes(iniPath, new byte[] { 0xFF, 0xFE });
            Ini iniFile = new Ini(iniPath);
            iniFile.Write("Global", "Version", "2");
            iniFile.Write("Global", "Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            iniFile.Write("Global", "Computer", Environment.MachineName);
            iniFile.Write("Global", "Destination", Destination);
            iniFile.Write("Global", "UserList", String.Join(",", Users));
            iniFile.Write("Global", "RecursiveCopy", RecursiveCopy.ToString());
            iniFile.Write("Global", "Compressed", Compress.ToString());

            foreach (string user in Users)
            {
                if (Desktop)
                    iniFile.Write(user, "Desktop", userData.GetUserPath(user, "Desktop"));
                if (Personal)
                    iniFile.Write(user, "Personal", userData.GetUserPath(user, "Personal"));

                if (NetworkDrives)
                {
                    try
                    {
                        int i = 0;
                        foreach (KeyValuePair<string, string> nDrv in userData.GetUserNetworkDrives(user))
                        {
                            iniFile.Write(user, "NetworkDrive-" + i, nDrv.Key + "||" + nDrv.Value);
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        copier.AddError("Could not read the network drives of " + user + ": " + ex.Message);
                    }
                }

                if (Printers)
                {
                    try
                    {
                        int i = 0;
                        foreach (string printer in userData.GetUserPrinterConnections(user))
                        {
                            iniFile.Write(user, "Printer-" + i, printer);
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        copier.AddError("Could not read the printers of " + user + ": " + ex.Message);
                    }
                }
            }

            // All the printers installed in the computer, for reference.
            if (Printers)
            {
                try
                {
                    int i = 0;
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
                    copier.AddError("Could not read the printers: " + ex.Message);
                }
            }

            int extra = 0;
            for (int j = 0; j < folders.Count; j++)
            {
                iniFile.Write("All", j.ToString(), folders[j].Source + "||" + folders[j].Target);
                if (folders[j].User == null)
                {
                    iniFile.Write("Extra", extra.ToString(), folders[j].Source + "||" + folders[j].Target);
                    extra++;
                }
            }

            copier.Compress = Compress;
            copier.TotalSize = GetTotalSize();

            foreach (BackupFolder folder in folders)
            {
                if (copier.Cancelled)
                    break;

                copier.CopyDirectory(folder.Source, LongPath.Combine(BackupPath, folder.Target), RecursiveCopy);
            }

            isDone = true;

            string[] errorList = Errors;
            if (errorList.Length > 0)
            {
                try
                {
                    logFile = Path.Combine(BackupPath, "Errors.log");
                    File.WriteAllLines(logFile, errorList);
                }
                catch (Exception)
                {
                    logFile = null;
                }
            }
        }
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
            return FileCopier.Measure(source.FullName, true);
        }
    }

    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class Ini
    {
        private const int BufferSize = 32767;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, char[] retVal, int size, string filePath);

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
            char[] buffer = new char[BufferSize];
            int length = GetPrivateProfileString(Section, Key, "", buffer, buffer.Length, this.path);
            return new string(buffer, 0, length);
        }

        /// <summary>
        /// Names of all the sections.
        /// </summary>
        public string[] GetSections()
        {
            return ReadList(null);
        }

        /// <summary>
        /// Names of all the keys of a section.
        /// </summary>
        public string[] GetKeys(string Section)
        {
            return ReadList(Section);
        }

        private string[] ReadList(string section)
        {
            char[] buffer = new char[BufferSize];
            int length = GetPrivateProfileString(section, null, "", buffer, buffer.Length, this.path);
            if (length <= 0)
                return new string[0];

            return new string(buffer, 0, length).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
