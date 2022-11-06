using System.Collections.Generic;
using System.IO;

namespace OpenSage.IO
{
    public sealed class DiskFileSystem : FileSystem
    {
        public readonly string RootDirectory;

        public DiskFileSystem(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public override IEnumerable<FileSystemEntry> GetFilesInDirectory(
            string directoryPath,
            string searchPattern,
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var childDirectory = Path.Combine(RootDirectory, NormalizeFilePath(directoryPath));

            var childDirectoryInfo = new DirectoryInfo(childDirectory);
            if (!childDirectoryInfo.Exists)
            {
                yield break;
            }

            var enumerationOptions = new EnumerationOptions
            {
                RecurseSubdirectories = searchOption == SearchOption.AllDirectories,
                MatchCasing = MatchCasing.CaseInsensitive,
            };

            foreach (var fileInfo in childDirectoryInfo.EnumerateFiles(searchPattern, enumerationOptions))
            {
                yield return CreateFileSystemEntry(fileInfo);
            }
        }

        public override FileSystemEntry GetFile(string filePath)
        {
            var fullFilePath = Path.Combine(RootDirectory, NormalizeFilePath(filePath));
            var fileInfo = new FileInfo(fullFilePath);
            if (!fileInfo.Exists)
            {
                return null;
            }

            return CreateFileSystemEntry(fileInfo);
        }

        private FileSystemEntry CreateFileSystemEntry(FileInfo fileInfo)
        {
            var fullPath = fileInfo.FullName;

            var relativePath = Path.GetRelativePath(
                RootDirectory,
                fullPath);

            return new FileSystemEntry(
                this,
                relativePath,
                (uint) fileInfo.Length,
                () => File.OpenRead(fullPath));
        }

        public string GetFullPath(FileSystemEntry entry)
        {
            return Path.Combine(RootDirectory, entry.FilePath);
        }
    }
}
