using System;
using System.IO;
using System.Linq;

namespace OpenSage.Data.IO
{
    public class FileProvider : AFileProviderBase
    {
        public static readonly char VolumeSeparatorChar = Path.VolumeSeparatorChar;
        public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;
        public static readonly char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;

        private readonly string _basePath;

        public FileProvider(string rootPath, string basePath) : base(rootPath)
        {
            _basePath = basePath;
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
                return new FileStream(ConvertUrlToPath(url), (System.IO.FileMode) mode, (System.IO.FileAccess) access, (System.IO.FileShare) share);
            }
            return base.OpenStream(url, mode, access, share);
        }

        public override string[] ListFiles(string url, string searchPattern, SearchOption searchOption)
        {
            if (DirectoryExists(url, true))
            {
                return Directory.GetFiles(ConvertUrlToPath(url), searchPattern, (System.IO.SearchOption) searchOption)
                                .Select(ConvertPathToUrl)
                                .Concat(base.ListFiles(url, searchPattern, searchOption))
                                .ToArray();
            }
            return base.ListFiles(url, searchPattern, searchOption);
        }

        public override void CreateDirectory(string url)
        {
            var path = ConvertUrlToPath(url);
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not create directory '{path}'.", ex);
            }
        }

        public override bool DirectoryExists(string url, bool topOnly = false)
        {
            if (Directory.Exists(ConvertUrlToPath(url)))
            {
                return true;
            }
            return topOnly ? false : base.DirectoryExists(url);
        }

        public override bool FileExists(string url, bool topOnly = false)
        {
            if (File.Exists(ConvertUrlToPath(url)))
            {
                return true;
            }
            return topOnly ? false : base.FileExists(url);
        }

        public override void FileDelete(string url)
        {
            if (File.Exists(ConvertUrlToPath(url)))
            {
                File.Delete(ConvertUrlToPath(url));
                return;
            }
            base.FileDelete(url);
        }
    }
}
