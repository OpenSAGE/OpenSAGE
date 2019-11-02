using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using OpenSage.FileFormats.Big;

namespace OpenSage.Data.IO
{
    public class BigProvider : AFileProviderBase
    {
        public static readonly char DirectorySeparatorChar = '\\';
        public static readonly char AltDirectorySeparatorChar = '/';

        private readonly BigArchive _bigFile;
        private string _basePath;

        public BigProvider(string rootPath, BigArchive bigfile) : base(rootPath)
        {
            _bigFile = bigfile;
            if (!(_basePath is null))
            {
                _basePath = _basePath.Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);
                if (!_basePath.EndsWith(DirectorySeparatorChar))
                {
                    _basePath += DirectorySeparatorChar;
                }
            }
        }

        private string ConvertUrlToPath(string url)
        {
            if (_basePath is null)
            {
                return url.Replace(FileSystem.DirectorySeparatorChar, DirectorySeparatorChar);
            }
            return _basePath + url.Replace(FileSystem.DirectorySeparatorChar, DirectorySeparatorChar);
        }

        private string ConvertPathToUrl(string path)
        {
            if (_basePath is null)
            {
                return path.Replace(DirectorySeparatorChar, FileSystem.DirectorySeparatorChar);
            }
            if (!path.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Path '{path}' does not belong to this provider.");
            }
            return path.Substring(_basePath.Length).Replace(DirectorySeparatorChar, FileSystem.DirectorySeparatorChar);
        }

        public override Stream OpenStream(string url, FileMode mode, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
        {
            if (FileExists(url, true))
            {
                return _bigFile.GetEntry(ConvertUrlToPath(url)).Open();
            }
            return base.OpenStream(url, mode, access, share);
        }

        public override string[] ListFiles(string url, string searchPattern, SearchOption searchOption)
        {
            var path = ConvertUrlToPath(url);
            if (path.Length > 0 && !path.EndsWith(DirectorySeparatorChar.ToString()))
            {
                path += DirectorySeparatorChar;
            }
            var result = new List<string>();
            foreach (var file in _bigFile.Entries)
            {
                if ((searchOption == SearchOption.AllDirectories || file.FullName.LastIndexOf(DirectorySeparatorChar) < path.Length)
                    && file.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
                    && Regex.IsMatch(file.FullName.Substring(path.Length), $"^{Regex.Escape(searchPattern).Replace("\\?", ".").Replace("\\*", ".*")}$"))
                {
                    result.Add(ConvertPathToUrl(file.FullName));
                }
            }
            result.AddRange(base.ListFiles(url, searchPattern, searchOption));
            return result.ToArray();
        }

        public override void CreateDirectory(string url)
        {
            throw new InvalidOperationException();
        }

        public override bool DirectoryExists(string url, bool topOnly = false)
        {
            string path = ConvertUrlToPath(url);
            foreach (BigArchiveEntry entry in _bigFile.Entries)
            {
                if (entry.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return topOnly ? false : base.DirectoryExists(url);
        }

        public override bool FileExists(string url, bool topOnly = false)
        {
            if (_bigFile.GetEntry(ConvertUrlToPath(url)) is null)
            {
                return topOnly ? false : base.FileExists(url);
            }
            return true;
        }

        public override void FileDelete(string url)
        {
            throw new InvalidOperationException();
        }
    }
}
