using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace WUMT
{
    /// <summary>
    /// A file or folder found by LongPath.GetEntries.
    /// </summary>
    public class FileEntry
    {
        public string Name;
        public string FullPath;
        public FileAttributes Attributes;
        public long Size;
        public FILETIME CreationTime;
        public FILETIME LastAccessTime;
        public FILETIME LastWriteTime;

        public bool IsDirectory
        {
            get { return (Attributes & FileAttributes.Directory) != 0; }
        }

        public bool IsReparsePoint
        {
            get { return (Attributes & FileAttributes.ReparsePoint) != 0; }
        }
    }

    /// <summary>
    /// File operations that work with paths longer than 260 characters.
    /// System.IO in .NET 3.5 doesn't support them, so this calls the Windows API
    /// with the "\\?\" prefix.
    /// </summary>
    public static class LongPath
    {
        private const uint INVALID_FILE_ATTRIBUTES = 0xFFFFFFFF;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_NO_MORE_FILES = 18;
        private const int ERROR_ALREADY_EXISTS = 183;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFileAttributesW(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetFileAttributesW(string lpFileName, uint dwFileAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreateDirectoryW(string lpPathName, IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CopyFileW(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetFileTime(SafeFileHandle hFile, ref FILETIME lpCreationTime,
            ref FILETIME lpLastAccessTime, ref FILETIME lpLastWriteTime);

        /// <summary>
        /// Adds the "\\?\" prefix. The path must be absolute.
        /// </summary>
        public static string ToLong(string path)
        {
            if (path.StartsWith(@"\\?\"))
                return path;
            if (path.StartsWith(@"\\"))
                return @"\\?\UNC\" + path.Substring(2);
            return @"\\?\" + path;
        }

        public static string Combine(string folder, string name)
        {
            return folder.TrimEnd('\\') + @"\" + name.TrimStart('\\');
        }

        public static bool Exists(string path)
        {
            return GetFileAttributesW(ToLong(path)) != INVALID_FILE_ATTRIBUTES;
        }

        public static bool DirectoryExists(string path)
        {
            uint attr = GetFileAttributesW(ToLong(path));
            return attr != INVALID_FILE_ATTRIBUTES && (attr & (uint)FileAttributes.Directory) != 0;
        }

        public static bool FileExists(string path)
        {
            uint attr = GetFileAttributesW(ToLong(path));
            return attr != INVALID_FILE_ATTRIBUTES && (attr & (uint)FileAttributes.Directory) == 0;
        }

        public static void SetNormalAttributes(string path)
        {
            SetFileAttributesW(ToLong(path), (uint)FileAttributes.Normal);
        }

        /// <summary>
        /// Lists the files and folders inside a folder.
        /// </summary>
        public static List<FileEntry> GetEntries(string folder)
        {
            List<FileEntry> result = new List<FileEntry>();
            WIN32_FIND_DATA data;

            IntPtr handle = FindFirstFileW(ToLong(Combine(folder, "*")), out data);
            if (handle == INVALID_HANDLE_VALUE)
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR_FILE_NOT_FOUND)
                    return result; // Empty drive root.
                throw new Win32Exception(error);
            }

            try
            {
                do
                {
                    if (data.cFileName == "." || data.cFileName == "..")
                        continue;

                    FileEntry entry = new FileEntry();
                    entry.Name = data.cFileName;
                    entry.FullPath = Combine(folder, data.cFileName);
                    entry.Attributes = (FileAttributes)data.dwFileAttributes;
                    entry.Size = ((long)data.nFileSizeHigh << 32) | data.nFileSizeLow;
                    entry.CreationTime = data.ftCreationTime;
                    entry.LastAccessTime = data.ftLastAccessTime;
                    entry.LastWriteTime = data.ftLastWriteTime;
                    result.Add(entry);
                }
                while (FindNextFileW(handle, out data));

                int error = Marshal.GetLastWin32Error();
                if (error != ERROR_NO_MORE_FILES)
                    throw new Win32Exception(error);
            }
            finally
            {
                FindClose(handle);
            }

            return result;
        }

        /// <summary>
        /// Creates a folder and all its parents.
        /// </summary>
        public static void CreateDirectory(string path)
        {
            path = path.TrimEnd('\\');
            if (path.Length == 0 || DirectoryExists(path))
                return;

            int index = path.LastIndexOf('\\');
            if (index > 0)
            {
                string parent = path.Substring(0, index);
                // Stop at the drive ("C:") or the share ("\\server\share").
                if (!parent.EndsWith(":") && !(parent.StartsWith(@"\\") && parent.Split('\\').Length <= 4))
                    CreateDirectory(parent);
            }

            if (!CreateDirectoryW(ToLong(path), IntPtr.Zero))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != ERROR_ALREADY_EXISTS)
                    throw new Win32Exception(error, "Could not create " + path + ": " + new Win32Exception(error).Message);
            }
        }

        /// <summary>
        /// Copies a file keeping its dates and attributes.
        /// </summary>
        public static void CopyFile(string source, string destination, bool overwrite)
        {
            if (!CopyFileW(ToLong(source), ToLong(destination), !overwrite))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static FileStream OpenRead(string path)
        {
            SafeFileHandle handle = CreateFileW(ToLong(path), GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return new FileStream(handle, FileAccess.Read);
        }

        public static FileStream Create(string path)
        {
            SafeFileHandle handle = CreateFileW(ToLong(path), GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, CREATE_ALWAYS, 0, IntPtr.Zero);
            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return new FileStream(handle, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Sets the dates of an open file. Call it after writing, before closing.
        /// </summary>
        public static void SetTimes(FileStream stream, FileEntry source)
        {
            stream.Flush();
            FILETIME c = source.CreationTime, a = source.LastAccessTime, w = source.LastWriteTime;
            SetFileTime(stream.SafeFileHandle, ref c, ref a, ref w);
        }
    }
}
