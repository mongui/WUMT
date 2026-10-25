using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace WUMT
{
    /// <summary>
    /// Copies folders with progress, cancellation and error log.
    /// Optionally compresses (adds ".gz") or decompresses (removes ".gz") every file.
    /// Supports paths longer than 260 characters.
    /// </summary>
    public class FileCopier
    {
        public const string CompressedExtension = ".gz";

        /// <summary>Compress every copied file with GZip and add ".gz" to its name.</summary>
        public bool Compress = false;

        /// <summary>Decompress every ".gz" file and remove the extension.</summary>
        public bool Decompress = false;

        /// <summary>When false, existing files in the destination are kept.</summary>
        public bool Overwrite = true;

        private long totalSize = 0;
        private long copiedSize = 0;
        private volatile bool cancelRequested = false;
        private List<string> errors = new List<string>();

        public long TotalSize
        {
            get { return Interlocked.Read(ref totalSize); }
            set { Interlocked.Exchange(ref totalSize, value); }
        }

        public long CopiedSize
        {
            get { return Interlocked.Read(ref copiedSize); }
        }

        public int CopiedPercentage
        {
            get
            {
                long total = TotalSize;
                if (total <= 0)
                    return 0;

                return (int)Math.Min(100, Math.Max(0, CopiedSize * 100 / total));
            }
        }

        public bool Cancelled
        {
            get { return cancelRequested; }
        }

        public string[] Errors
        {
            get { lock (errors) return errors.ToArray(); }
        }

        public void Cancel()
        {
            cancelRequested = true;
        }

        public void AddError(string message)
        {
            lock (errors) errors.Add(message);
        }

        /// <summary>
        /// Size of the files in a folder. Skips junctions and folders that can't be read.
        /// </summary>
        public static long Measure(string folder, bool recursive)
        {
            long size = 0;
            List<FileEntry> entries;

            try
            {
                entries = LongPath.GetEntries(folder);
            }
            catch (Exception)
            {
                return 0;
            }

            foreach (FileEntry entry in entries)
            {
                if (!entry.IsDirectory)
                    size += entry.Size;
                else if (recursive && !entry.IsReparsePoint)
                    size += Measure(entry.FullPath, true);
            }

            return size;
        }

        /// <summary>
        /// Copies the files of a folder (and its subfolders if recursive).
        /// Junctions and symbolic links ("My Music" inside Documents, etc.) are skipped:
        /// they are usually not readable and point to folders that are copied anyway.
        /// </summary>
        public void CopyDirectory(string source, string destination, bool recursive)
        {
            List<FileEntry> entries;
            try
            {
                LongPath.CreateDirectory(destination);
                entries = LongPath.GetEntries(source);
            }
            catch (Exception ex)
            {
                AddError(source + ": " + ex.Message);
                return;
            }

            foreach (FileEntry entry in entries)
            {
                if (cancelRequested)
                    return;

                if (!entry.IsDirectory)
                    CopyFile(entry, destination);
            }

            if (!recursive)
                return;

            foreach (FileEntry entry in entries)
            {
                if (cancelRequested)
                    return;

                if (entry.IsDirectory && !entry.IsReparsePoint)
                    CopyDirectory(entry.FullPath, LongPath.Combine(destination, entry.Name), true);
            }
        }

        private void CopyFile(FileEntry file, string destinationFolder)
        {
            string name = file.Name;
            bool decompress = Decompress && name.EndsWith(CompressedExtension, StringComparison.OrdinalIgnoreCase);

            if (decompress)
                name = name.Substring(0, name.Length - CompressedExtension.Length);
            else if (Compress)
                name += CompressedExtension;

            string target = LongPath.Combine(destinationFolder, name);

            try
            {
                bool exists = LongPath.Exists(target);
                if (exists && !Overwrite)
                {
                    // Kept as it is.
                }
                else
                {
                    // Overwriting a read-only file fails, so clear the attribute first.
                    if (exists)
                        LongPath.SetNormalAttributes(target);

                    if (Compress || decompress)
                        CopyStream(file, target, Compress);
                    else
                        LongPath.CopyFile(file.FullPath, target, true);
                }
            }
            catch (Exception ex)
            {
                AddError(file.FullPath + ": " + ex.Message);
            }

            Interlocked.Add(ref copiedSize, file.Size);
        }

        private void CopyStream(FileEntry file, string target, bool compress)
        {
            byte[] buffer = new byte[81920];
            int read;

            using (FileStream input = LongPath.OpenRead(file.FullPath))
            using (FileStream output = LongPath.Create(target))
            {
                if (compress)
                {
                    using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress, true))
                    {
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (cancelRequested)
                                break;
                            gzip.Write(buffer, 0, read);
                        }
                    }
                }
                else
                {
                    using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress, true))
                    {
                        while ((read = gzip.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (cancelRequested)
                                break;
                            output.Write(buffer, 0, read);
                        }
                    }
                }

                LongPath.SetTimes(output, file);
            }
        }
    }
}
