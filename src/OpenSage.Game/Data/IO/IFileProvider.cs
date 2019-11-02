using System;
using System.IO;

namespace OpenSage.Data.IO
{
    public interface IFileProvider : IDisposable
    {
        string RootPath { get; }

        Stream OpenStream(string url, FileMode mode, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read);

        string[] ListFiles(string url, string searchPattern, SearchOption searchOption);

        void CreateDirectory(string url);

        bool DirectoryExists(string url, bool topOnly = false);

        bool FileExists(string url, bool topOnly = false);

        void FileDelete(string url);

        void AddSubProvider(IFileProvider provider);

        void RemoveSubProvider(IFileProvider provider);
    }
}
