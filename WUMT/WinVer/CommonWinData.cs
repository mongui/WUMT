using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace WUMT.WinVer
{
    public class CommonWinData
    {
        private const string ProfileListKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";

        private string SystemDrive;
        private string SystemRoot;

        // User name (profile folder name) -> { "SID", "ProfilePath", shell folders... }
        private Dictionary<string, Dictionary<string, string>> Users = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private bool isAdminSet;
        private bool isAdmin;

        /// <summary>
        /// Fills the "Users" dictionary with the name, SID and profile path of every user of the system.
        /// </summary>
        public void FillWindowsUserAccounts()
        {
            using (RegistryKey list = Registry.LocalMachine.OpenSubKey(ProfileListKey))
            {
                if (list == null)
                    return;

                foreach (string possUser in list.GetSubKeyNames())
                {
                    if (!possUser.StartsWith("S-1-5-21-"))
                        continue;

                    try
                    {
                        using (RegistryKey reg = list.OpenSubKey(possUser))
                        {
                            string pip = reg == null ? null : reg.GetValue("ProfileImagePath") as string;
                            if (String.IsNullOrEmpty(pip))
                                continue;

                            string profilePath = Environment.ExpandEnvironmentVariables(pip).TrimEnd('\\');
                            string name = profilePath.Substring(profilePath.LastIndexOf('\\') + 1);

                            if (Users.ContainsKey(name))
                                continue;

                            Dictionary<string, string> usr = new Dictionary<string, string>();
                            usr.Add("SID", possUser);
                            usr.Add("ProfilePath", profilePath);
                            Users.Add(name, usr);
                        }
                    }
                    catch (Exception)
                    { }
                }
            }
        }

        /// <summary>
        /// Recovers a list of Windows users from the registry.
        /// </summary>
        /// <returns>String array of Windows user accounts.</returns>
        public string[] GetWindowsUserAccounts()
        {
            if (Users.Count == 0)
                FillWindowsUserAccounts();

            return new List<string>(Users.Keys).ToArray();
        }

        /// <summary>
        /// Name used in the user list for the user running the application.
        /// It's the profile folder name, that can differ from the login name.
        /// </summary>
        public string GetCurrentUserName()
        {
            if (Users.Count == 0)
                FillWindowsUserAccounts();

            try
            {
                string sid = WindowsIdentity.GetCurrent().User.Value;
                foreach (KeyValuePair<string, Dictionary<string, string>> usr in Users)
                {
                    if (usr.Value["SID"] == sid)
                        return usr.Key;
                }
            }
            catch (Exception)
            { }

            return Environment.UserName;
        }

        /// <summary>
        /// Checks if the current user has administrative privileges.
        /// </summary>
        /// <returns>Is Admin?</returns>
        public bool IsUserAdministrator()
        {
            if (!isAdminSet)
            {
                try
                {
                    WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (Exception)
                {
                    isAdmin = false;
                }

                isAdminSet = true;
            }

            return isAdmin;
        }

        /// <summary>
        /// Gets SystemRoot variable from the Windows Registry.
        /// </summary>
        /// <returns>SystemRoot</returns>
        public string GetSystemRoot()
        {
            if (SystemRoot == null)
            {
                using (RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (reg != null)
                        SystemRoot = reg.GetValue("SystemRoot") as string;
                }

                if (SystemRoot == null)
                    SystemRoot = Environment.GetEnvironmentVariable("SystemRoot");
            }

            return SystemRoot;
        }

        /// <summary>
        /// Gets the unit drive where Windows is installed.
        /// </summary>
        /// <returns>Unit drive. ("C:")</returns>
        public string GetSystemDrive()
        {
            if (SystemDrive == null)
            {
                string sysDrive = Environment.GetEnvironmentVariable("SystemDrive");
                if (sysDrive == null)
                {
                    string sysRoot = GetSystemRoot();
                    if (sysRoot != null)
                        SystemDrive = Path.GetPathRoot(sysRoot).TrimEnd('\\').ToUpper();
                }
                else
                {
                    SystemDrive = sysDrive.ToUpper();
                }
            }

            return SystemDrive;
        }

        /// <summary>
        /// Gets the Windows shell folder of the current user.
        /// </summary>
        /// <param name="Folder">User Shell Folder</param>
        /// <returns>Path to the selected User Shell Folder</returns>
        public string GetUserPath(string Folder)
        {
            return GetUserPath(GetCurrentUserName(), Folder);
        }

        /// <summary>
        /// Gets the Windows shell folder of an specific user.
        /// Depending of the SO, these are the possible folders it can return:
        /// AppData, Cache, Cookies, Desktop, Favorites, History, LocalAppData, My Pictures, NetHood,
        /// Personal, PrintHood, Programs, Recent, SendTo, Start Menu, Startup, Templates.
        /// </summary>
        /// <param name="user">Windows user name</param>
        /// <param name="Folder">User Shell Folder</param>
        /// <returns>Path to the selected User Shell Folder, or null if not found.</returns>
        public string GetUserPath(string user, string Folder)
        {
            if (Users.Count == 0)
                FillWindowsUserAccounts();

            if (user == null || !Users.ContainsKey(user))
                return null;

            Dictionary<string, string> usr = Users[user];

            if (!usr.ContainsKey(Folder))
                usr[Folder] = ReadUserShellFolder(usr["SID"], usr["ProfilePath"], Folder);

            return usr[Folder];
        }

        private static string ReadUserShellFolder(string sid, string profilePath, string folder)
        {
            string value = null;

            // The user's registry hive is only loaded while the user is logged in.
            try
            {
                using (RegistryKey reg = Registry.Users.OpenSubKey(sid + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders"))
                {
                    if (reg != null)
                        value = reg.GetValue(folder, null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
                }
            }
            catch (Exception)
            { }

            if (!String.IsNullOrEmpty(value))
            {
                // %USERPROFILE% must point to that user, not to the one running the application.
                value = Regex.Replace(value, Regex.Escape("%USERPROFILE%"), profilePath.Replace("$", "$$"), RegexOptions.IgnoreCase);
                return Environment.ExpandEnvironmentVariables(value);
            }

            // Default locations when the hive isn't available.
            string[] candidates;
            switch (folder)
            {
                case "Desktop":
                    candidates = new string[] { "Desktop" };
                    break;
                case "Personal":
                    candidates = new string[] { "Documents", "My Documents" };
                    break;
                default:
                    return null;
            }

            foreach (string candidate in candidates)
            {
                string path = Path.Combine(profilePath, candidate);
                if (Directory.Exists(path))
                    return path;
            }

            return null;
        }

        /// <summary>
        /// Searches in the Windows Registry and returns the Product Key used in the machine.
        /// </summary>
        /// <returns>The Windows Product Key stored in the current machine.</returns>
        public string GetWindowsProductKey()
        {
            try
            {
                using (RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (reg == null)
                        return null;

                    byte[] DPId = reg.GetValue("DigitalProductId") as byte[];
                    return DPId == null ? null : DecodeProductKey(DPId);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the encoded Windows Product Key and decodes it.
        /// Supports both the old format and the one used since Windows 8.
        /// </summary>
        /// <param name="digitalProductId">A bunch of bytes obtained from the Windows Registry that contains the encoded Product Key.</param>
        /// <returns>The decoded Product Key.</returns>
        private string DecodeProductKey(byte[] digitalProductId)
        {
            const string digits = "BCDFGHJKMPQRTVWXY2346789";
            const int keyOffset = 52;

            byte[] key = new byte[15];
            Array.Copy(digitalProductId, keyOffset, key, 0, 15);

            // Windows 8 and later mark the new format in this bit.
            int isWin8 = (key[14] / 6) & 1;
            key[14] = (byte)((key[14] & 0xF7) | ((isWin8 & 2) * 4));

            char[] chars = new char[25];
            int last = 0;
            for (int i = 24; i >= 0; i--)
            {
                int current = 0;
                for (int j = 14; j >= 0; j--)
                {
                    current = current * 256 + key[j];
                    key[j] = (byte)(current / 24);
                    current %= 24;
                }
                chars[i] = digits[current];
                last = current;
            }

            string decoded = new string(chars);

            if (isWin8 == 1)
            {
                // In the new format the position of the "N" is stored in the first character.
                decoded = decoded.Substring(1, last) + "N" + decoded.Substring(last + 1);
            }

            return decoded.Substring(0, 5) + "-" + decoded.Substring(5, 5) + "-" + decoded.Substring(10, 5)
                + "-" + decoded.Substring(15, 5) + "-" + decoded.Substring(20, 5);
        }

        /// <summary>
        /// Gets all network drives of the current Windows user.
        /// </summary>
        /// <returns>Dictionary(Drive Letter, Network route)</returns>
        public Dictionary<string, string> GetMappedNetworkDrives()
        {
            Dictionary<string, string> networkDrives = new Dictionary<string, string>();
            foreach (DriveInfo di in DriveInfo.GetDrives())
            {
                if (di.DriveType == DriveType.Network)
                {
                    string letter = di.Name.Substring(0, 2);
                    networkDrives[letter] = GetUNCPath(letter);
                }
            }

            return networkDrives;
        }

        /// <summary>
        /// Translates a network drive letter to its network route.
        /// </summary>
        /// <param name="path">Drive letter (Z:)</param>
        /// <returns>Network route (\\192.168.0.1\Folder)</returns>
        private string GetUNCPath(string path)
        {
            if (path.StartsWith(@"\\")) return path;

            using (ManagementObject mo = new ManagementObject())
            {
                mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", path));

                //DriveType 4 = Network Drive
                if (Convert.ToUInt32(mo["DriveType"]) == 4) return Convert.ToString(mo["ProviderName"]);
                else return path;
            }
        }

        /// <summary>
        /// Gets the installed printers.
        /// </summary>
        /// <returns>Dictionary("Local"/"Network", printer names)</returns>
        public Dictionary<string, string[]> GetMappedPrinters()
        {
            List<string> prnLocal = new List<string>();
            List<string> prnNetwork = new List<string>();

            using (ManagementObjectSearcher objMOS = new ManagementObjectSearcher("SELECT Name, Local, Network FROM Win32_Printer"))
            {
                foreach (ManagementObject printer in objMOS.Get())
                {
                    string name = Convert.ToString(printer["Name"]);

                    if (Convert.ToBoolean(printer["Network"]))
                        prnNetwork.Add(name);
                    else if (Convert.ToBoolean(printer["Local"]))
                        prnLocal.Add(name);
                }
            }

            Dictionary<string, string[]> printers = new Dictionary<string, string[]>();
            printers.Add("Local", prnLocal.ToArray());
            printers.Add("Network", prnNetwork.ToArray());

            return printers;
        }
    }
}
