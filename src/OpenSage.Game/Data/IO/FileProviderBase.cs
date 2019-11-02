using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.IO
{
    public abstract class AFileProviderBase : IFileProvider
    {
        private readonly List<IFileProvider> _subProviders = new List<IFileProvider>();

        public string RootPath { get; private set; }

        protected AFileProviderBase(string rootPath)
        {
            if (!(rootPath is null))
            {
                if (rootPath == string.Empty)
                {
                    throw new ArgumentException(nameof(rootPath));
                }
                if (rootPath[rootPath.Length - 1] != FileSystem.DirectorySeparatorChar)
                {
                    rootPath += FileSystem.DirectorySeparatorChar;
                }
            }
            RootPath = rootPath;
            FileSystem.RegisterProvider(this);
        }

        public virtual Stream OpenStream(string url, FileMode mode, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
        {
            foreach (var provider in _subProviders)
            {
                if (provider.FileExists(url))
                {
                    return provider.OpenStream(url, mode, access, share);
                }
            }
            return null;
        }

        public virtual string[] ListFiles(string url, string searchPattern, SearchOption searchOption)
        {
            var result = new List<string>();
            foreach (var provider in _subProviders)
            {
                result.AddRange(provider.ListFiles(url, searchPattern, searchOption));
            }
            return result.ToArray();
        }

        public virtual void CreateDirectory(string url)
        {
            throw new NotImplementedException();
        }

        public virtual bool DirectoryExists(string url, bool topOnly = false)
        {
            if (topOnly)
            {
                throw new InvalidOperationException();
            }
            foreach (var provider in _subProviders)
            {
                if (provider.DirectoryExists(url))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool FileExists(string url, bool topOnly = false)
        {
            if (topOnly)
            {
                throw new InvalidOperationException();
            }
            foreach (var provider in _subProviders)
            {
                if (provider.FileExists(url))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void FileDelete(string url)
        {
            foreach (var provider in _subProviders)
            {
                if (provider.DirectoryExists(url))
                {
                    provider.FileDelete(url);
                    return;
                }
            }
        }

        public void AddSubProvider(IFileProvider provider)
        {
            _subProviders.Add(provider);
        }

        public void RemoveSubProvider(IFileProvider provider)
        {
            _subProviders.Remove(provider);
        }

        public virtual void Dispose()
        {
            FileSystem.UnregisterProvider(this);
            foreach (var provider in _subProviders)
            {
                provider.Dispose();
            }
        }

        public override string ToString()
        {
            return RootPath;
        }
    }
}
