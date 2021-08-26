using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using OpenSage.Data;

namespace OpenSage.Tools.AptEditor.Util
{
    internal static class FileUtilities
    {
        public static IEnumerable<FileSystemEntry> FindFiles(this FileSystem? fileSystem, Func<FileSystemEntry, bool> matcher)
        {
            if (fileSystem is null)
            {
                return Enumerable.Empty<FileSystemEntry>();
            }
            return fileSystem.Files
                .Where(matcher)
                .Concat(fileSystem.NextFileSystem.FindFiles(matcher));
        }

        public static (Func<Stream>, uint) GetFile2(this FileSystem target, string path, bool isPhysicalFile)
        {
            if (isPhysicalFile)
            {
                var info = new FileInfo(path);
                return (info.OpenRead, (uint) info.Length);
            }
            var entry = target.GetFile(path);
            return (entry.Open, entry.Length);
        }

        public static void LoadFiles(
            this FileSystem target,
            IEnumerable<(string, string)> listAndMapped,
            bool isPhysicalFile,
            bool loadArtOnly)
        {
            var art = FileSystem.NormalizeFilePath("art/textures/");
            foreach (var (from, to) in listAndMapped)
            {
                var (open, length) = target.GetFile2(from, isPhysicalFile);
                if (!loadArtOnly)
                {
                    target.Update(new FileSystemEntry(target,
                                                                FileSystem.NormalizeFilePath(to),
                                                                length,
                                                                open));
                }

                // load art
                var normalizedArt = FileSystem.NormalizeFilePath(from);
                var index = normalizedArt.IndexOf(art);
                if (index == -1)
                {
                    continue;
                }
                target.Update(new FileSystemEntry(target,
                                                            normalizedArt[index..],
                                                            length,
                                                            open));
            }
        }

        public static IEnumerable<string> GetFilesByDirectory(
            string? directoryIn,
            int maxCount = -1,
            Func<string, bool>? filter = null,
            Action? throwIfCancellationRequested = null
            )
        {
            var dir = Directory.Exists(directoryIn)
                ? directoryIn
                : Path.GetDirectoryName(directoryIn);

            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                return Enumerable.Empty<string>();
            }

            var ret = new List<string>();
            var max = maxCount;
            if (max < 0)
                max = 0x7fffffff;

            var files = Directory.EnumerateFiles(dir, "*", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false
            });

            if (throwIfCancellationRequested != null)
                throwIfCancellationRequested();

            foreach (var file in files)
            {
                if (throwIfCancellationRequested != null)
                    throwIfCancellationRequested();
                if (filter == null || (filter != null && filter(file)))
                {
                    ret.Add(Path.GetRelativePath(dir, file));
                    if (ret.Count >= max)
                        break;
                }
            }

            return ret.AsReadOnly();
        }

        public static IEnumerable<(string, string)> GetFilesMappingByDirectory(
            string orig,
            string mapped,
            out IEnumerable<string> origPathsOut,
            out IEnumerable<string> mappedPathsOut, 
            int maxCount = -1,
            Func<string, bool>? filter = null,
            Action? throwIfCancellationRequested = null
            )
        {
            var origPaths = GetFilesByDirectory(
                orig,
                maxCount: maxCount,
                filter: filter,
                throwIfCancellationRequested: throwIfCancellationRequested
                );
            origPathsOut = origPaths;

            var mappedPaths = new List<string>();
            foreach (var path in origPaths)
                mappedPaths.Add(string.IsNullOrWhiteSpace(mapped) ? path : Path.Combine(mapped, path));
            mappedPathsOut = mappedPaths.AsReadOnly();
            
            var ret = origPaths.Select(from => Path.Combine(orig, from)).Zip(mappedPaths);

            return ret;
        }
    }
}
