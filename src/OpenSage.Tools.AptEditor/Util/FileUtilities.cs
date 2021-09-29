using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using OpenSage.Data;
using OpenSage.FileFormats.Apt;
using System.Diagnostics.CodeAnalysis;

namespace OpenSage.Tools.AptEditor.Util
{
    public static class FileUtilities
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

        public static string NormalizePath(this string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       // is it necessary?
                       // .ToLowerInvariant()
                       ;
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

        // apt related

        public static void CheckImportTree(this FileSystem TargetFileSystem, AptFile apt)
        {
            foreach (var import in apt.Movie.Imports)
            {
                var importPath = Path.Combine(apt.RootDirectory, Path.ChangeExtension(import.Movie, ".apt"));
                var importEntry = TargetFileSystem.GetFile(importPath);
                if (importEntry == null)
                    throw new FileNotFoundException("Cannot find imported file", importPath);

                // Some dirty tricks to avoid the above exception
                var getter = new StandardStreamGetter(importPath, (s, m) => TargetFileSystem.GetFile(s).Open());
                var f = AptFile.Parse(getter);
                TargetFileSystem.CheckImportTree(f);
            }
        }

        public static AptFile LoadApt(this FileSystem TargetFileSystem, string path)
        {
            var entry = TargetFileSystem.GetFile(path);
            if (entry == null)
            {
                throw new FileNotFoundException("Cannot find file", path);
            }

            TargetFileSystem.AutoLoad(path, loadArtOnly: true); // here it's used to prepare art folder
            var aptFile = AptFileHelper.FromFileSystemEntry(entry);

            return aptFile;
        }

        [SuppressMessage("Microsoft.Performance", "CA2200", MessageId = "intentional")]
        public static void LoadImportTree(this FileSystem TargetFileSystem, AptFile apt)
        {
            string? lastFailed = null;
            while (true)
            {
                try
                {
                    TargetFileSystem.CheckImportTree(apt);
                }
                catch (FileNotFoundException loadFailure)
                {
                    if (loadFailure.FileName is string file)
                    {
                        if (file != lastFailed)
                        {
                            lastFailed = file;
                            if (TargetFileSystem.AutoLoad(file, loadArtOnly: false))
                            {
                                continue;
                            }
                        }
                    }
                    throw loadFailure;
                }
                break;
            }
        }

        public static bool AutoLoad(this FileSystem TargetFileSystem, string requiredAptFile, bool loadArtOnly)
        {
            requiredAptFile = FileSystem.NormalizeFilePath(requiredAptFile);
            var mappedPath = Path.GetDirectoryName(requiredAptFile);
            var name = Path.GetFileName(requiredAptFile);

            var detectedFromGameFileSystem = TargetFileSystem
                .FindFiles(entry => Path.GetFileName(entry.FilePath) == name)
                .Select(entry => entry.FilePath)
                .ToArray();

            if (!detectedFromGameFileSystem.Any())
            {
                return false;
            }

            void Trace(string sourcePath, string from)
            {
                var text = $"Automatically loaded game:{sourcePath}  => game:{mappedPath} for {requiredAptFile}";
                Console.WriteLine(text + (loadArtOnly ? " (art)" : string.Empty));
            }

            foreach (var gameFile in detectedFromGameFileSystem)
            {
                var sourcePath = Path.GetDirectoryName(gameFile);
                TargetFileSystem.AutoLoadImpl(sourcePath!, mappedPath, loadArtOnly);
                Trace(sourcePath!, "game");
            }

            return true;
        }

        public static void AutoLoadImpl(this FileSystem TargetFileSystem,
                                        string sourcePath,
                                        string? mappedPath,
                                        bool loadArtOnly)
        {
            if (!Path.EndsInDirectorySeparator(sourcePath))
            {
                sourcePath += Path.DirectorySeparatorChar;
            }
            var paths = TargetFileSystem.FindFiles(_ => true).Select(entry => entry.FilePath);

            var normalizedSourcePath = FileSystem.NormalizeFilePath(sourcePath);
            var filtered = from path in paths
                           let normalized = FileSystem.NormalizeFilePath(path)
                           where normalized.StartsWith(normalizedSourcePath)
                           let relative = normalized[(normalizedSourcePath.Length)..]
                           let mapped = mappedPath is null
                               ? relative
                               : Path.Combine(mappedPath, relative)
                           select (path, mapped);
            TargetFileSystem.LoadFiles(filtered.ToArray(), false, loadArtOnly);
        }
    }
}
