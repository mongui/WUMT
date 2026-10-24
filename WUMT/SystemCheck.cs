using Microsoft.Win32;
using System;

namespace WUMT
{
    class SystemCheck
    {
        public WinType OS;
        public enum WinType
        {
            Unknown,
            Win95,
            Win98,
            Win98SE,
            WinMe,
            WinNT351,
            WinNT4,
            Win2000,
            WinXP,
            Win2003,
            WinVista,
            Win7,
            Win8,
            Win81,
            Win10,
            Win11
        }

        public SystemCheck() {
            DetermineOS();
        }

        /// <summary>
        /// Defines the OS.
        /// </summary>
        private void DetermineOS()
        {
            OS = WinType.Unknown;
            Version version = Environment.OSVersion.Version;

            switch (Environment.OSVersion.Platform)
            {
                // Win95/Win98/Win98SE/WinMe.
                case PlatformID.Win32Windows:
                    switch (version.Minor)
                    {
                        case 0:
                            OS = WinType.Win95;
                            break;
                        case 10:
                            OS = (version.Build == 2222) ? WinType.Win98SE : WinType.Win98;
                            break;
                        case 90:
                            OS = WinType.WinMe;
                            break;
                    }
                    break;

                // WinNT351/WinNT4/Win2000/WinXP/Win2003/WinVista/Win7/Win8/Win81/Win10/Win11.
                case PlatformID.Win32NT:
                    // Without a compatibility manifest, Windows 8.1 and later report 6.2,
                    // so read the real version from the registry.
                    version = GetRealNTVersion(version);

                    switch (version.Major)
                    {
                        case 3:
                            OS = WinType.WinNT351;
                            break;
                        case 4:
                            OS = WinType.WinNT4;
                            break;
                        case 5:
                            switch (version.Minor)
                            {
                                case 0:
                                    OS = WinType.Win2000;
                                    break;
                                case 1:
                                    OS = WinType.WinXP;
                                    break;
                                case 2:
                                    OS = WinType.Win2003;
                                    break;
                            }
                            break;
                        case 6:
                            switch (version.Minor)
                            {
                                case 0:
                                    OS = WinType.WinVista;
                                    break;
                                case 1:
                                    OS = WinType.Win7;
                                    break;
                                case 2:
                                    OS = WinType.Win8;
                                    break;
                                case 3:
                                    OS = WinType.Win81;
                                    break;
                            }
                            break;
                        case 10:
                            OS = (version.Build >= 22000) ? WinType.Win11 : WinType.Win10;
                            break;
                    }
                    break;
            }
        }

        private static Version GetRealNTVersion(Version reported)
        {
            try
            {
                using (RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (reg == null)
                        return reported;

                    // Windows 10 and later.
                    object major = reg.GetValue("CurrentMajorVersionNumber");
                    object minor = reg.GetValue("CurrentMinorVersionNumber");
                    int build;
                    int.TryParse(reg.GetValue("CurrentBuildNumber") as string, out build);

                    if (major is int && minor is int)
                        return new Version((int)major, (int)minor, build);

                    // Windows 8.1 and earlier: "6.3".
                    string current = reg.GetValue("CurrentVersion") as string;
                    if (current != null)
                    {
                        string[] parts = current.Split('.');
                        int ma, mi;
                        if (parts.Length == 2 && int.TryParse(parts[0], out ma) && int.TryParse(parts[1], out mi))
                            return new Version(ma, mi, build);
                    }
                }
            }
            catch (Exception)
            { }

            return reported;
        }
    }
}
