using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WUMT.WinVer
{
    /// <summary>
    /// Opens the registry (HKEY_CURRENT_USER) of any user.
    /// If the user is logged in, its hive is already in HKEY_USERS\SID.
    /// Otherwise its NTUSER.DAT is loaded temporarily, which needs administrator rights.
    /// Also works with the NTUSER.DAT of a disk from another computer.
    /// Dispose it to close the key and unload the hive.
    /// </summary>
    public class UserHive : IDisposable
    {
        private static readonly UIntPtr HKEY_USERS = new UIntPtr(0x80000003u);
        private static readonly object loadLock = new object();
        private static bool privilegesEnabled = false;

        private RegistryKey root;
        private string loadedName;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegLoadKey(UIntPtr hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegUnLoadKey(UIntPtr hKey, string lpSubKey);

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID Luid;
            public uint Attributes;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private UserHive(RegistryKey root, string loadedName)
        {
            this.root = root;
            this.loadedName = loadedName;
        }

        /// <summary>
        /// The user's HKEY_CURRENT_USER.
        /// </summary>
        public RegistryKey Root
        {
            get { return root; }
        }

        /// <summary>
        /// Opens the registry of a user of this computer, or null if not possible.
        /// </summary>
        public static UserHive Open(string sid, string profilePath)
        {
            if (!String.IsNullOrEmpty(sid))
            {
                try
                {
                    RegistryKey key = Registry.Users.OpenSubKey(sid, true) ?? Registry.Users.OpenSubKey(sid);
                    if (key != null)
                        return new UserHive(key, null);
                }
                catch (Exception)
                { }
            }

            if (String.IsNullOrEmpty(profilePath))
                return null;

            return Load(Path.Combine(profilePath, "NTUSER.DAT"));
        }

        /// <summary>
        /// Loads a NTUSER.DAT file, or returns null if not possible.
        /// </summary>
        public static UserHive Load(string hiveFile)
        {
            if (!File.Exists(hiveFile))
                return null;

            lock (loadLock)
            {
                EnablePrivileges();

                string name = "WUMT_" + Guid.NewGuid().ToString("N");
                if (RegLoadKey(HKEY_USERS, name, hiveFile) != 0)
                    return null;

                RegistryKey key = null;
                try
                {
                    key = Registry.Users.OpenSubKey(name, true);
                }
                catch (Exception)
                { }

                if (key == null)
                {
                    RegUnLoadKey(HKEY_USERS, name);
                    return null;
                }

                return new UserHive(key, name);
            }
        }

        public void Dispose()
        {
            if (root != null)
            {
                root.Close();
                root = null;
            }

            if (loadedName != null)
            {
                // Handles to the hive may still be pending finalization.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                RegUnLoadKey(HKEY_USERS, loadedName);
                loadedName = null;
            }
        }

        /// <summary>
        /// Reads a value of the user's "User Shell Folders" without expanding variables.
        /// </summary>
        public string GetShellFolder(string folder)
        {
            using (RegistryKey reg = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders"))
            {
                if (reg == null)
                    return null;
                return reg.GetValue(folder, null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
            }
        }

        /// <summary>
        /// Persistent mapped network drives: letter ("Z:") and network path.
        /// </summary>
        public Dictionary<string, string> GetNetworkDrives()
        {
            Dictionary<string, string> drives = new Dictionary<string, string>();

            using (RegistryKey network = root.OpenSubKey("Network"))
            {
                if (network == null)
                    return drives;

                foreach (string letter in network.GetSubKeyNames())
                {
                    using (RegistryKey drive = network.OpenSubKey(letter))
                    {
                        string remote = drive == null ? null : drive.GetValue("RemotePath") as string;
                        if (!String.IsNullOrEmpty(remote))
                            drives[letter.ToUpper() + ":"] = remote;
                    }
                }
            }

            return drives;
        }

        /// <summary>
        /// Writes a persistent mapped drive. Windows connects it at the next logon.
        /// </summary>
        public void AddNetworkDrive(string letter, string remotePath)
        {
            using (RegistryKey drive = root.CreateSubKey(@"Network\" + letter.TrimEnd(':').ToUpper()))
            {
                drive.SetValue("RemotePath", remotePath, RegistryValueKind.String);
                drive.SetValue("UserName", "", RegistryValueKind.String);
                drive.SetValue("ProviderName", "Microsoft Windows Network", RegistryValueKind.String);
                drive.SetValue("ProviderType", 0x20000, RegistryValueKind.DWord);
                drive.SetValue("ConnectionType", 1, RegistryValueKind.DWord);
                drive.SetValue("DeferFlags", 4, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Network printer connections of the user ("\\server\printer").
        /// </summary>
        public List<string> GetPrinterConnections()
        {
            List<string> printers = new List<string>();

            using (RegistryKey connections = root.OpenSubKey(@"Printers\Connections"))
            {
                if (connections == null)
                    return printers;

                // Keys are named ",,server,printer".
                foreach (string name in connections.GetSubKeyNames())
                {
                    string[] parts = name.Split(new char[] { ',' }, 4);
                    if (parts.Length == 4 && parts[2].Length > 0)
                        printers.Add(@"\\" + parts[2] + @"\" + parts[3]);
                }
            }

            return printers;
        }

        /// <summary>
        /// Writes a network printer connection. Windows connects it at the next logon.
        /// </summary>
        public void AddPrinterConnection(string printer)
        {
            string[] parts = printer.TrimStart('\\').Split(new char[] { '\\' }, 2);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid printer name: " + printer);

            using (RegistryKey connection = root.CreateSubKey(@"Printers\Connections\,," + parts[0] + "," + parts[1]))
            {
                connection.SetValue("Server", @"\\" + parts[0], RegistryValueKind.String);
                connection.SetValue("Provider", "win32spl.dll", RegistryValueKind.String);
            }
        }

        private static void EnablePrivileges()
        {
            if (privilegesEnabled)
                return;

            const uint TOKEN_ADJUST_PRIVILEGES = 0x20, TOKEN_QUERY = 0x8, SE_PRIVILEGE_ENABLED = 0x2;

            IntPtr token;
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out token))
                return;

            try
            {
                foreach (string privilege in new string[] { "SeRestorePrivilege", "SeBackupPrivilege" })
                {
                    TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES();
                    tp.PrivilegeCount = 1;
                    tp.Attributes = SE_PRIVILEGE_ENABLED;
                    if (LookupPrivilegeValue(null, privilege, out tp.Luid))
                        AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                }
            }
            finally
            {
                CloseHandle(token);
            }

            privilegesEnabled = true;
        }
    }
}
