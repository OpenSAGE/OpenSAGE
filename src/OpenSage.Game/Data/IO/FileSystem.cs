using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSage.Data.IO
{
    // Copy of System.IO enum, so one doesn't need to include 2 namespaces to use the FileSystem
    public enum FileMode
    {
        CreateNew = 1,
        Create,
        Open,
        OpenOrCreate,
        Truncate,
        Append
    }

    // Copy of System.IO enum, so one doesn't need to include 2 namespaces to use the FileSystem
    [Flags]
    public enum FileAccess
    {
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write
    }

    // Copy of System.IO enum, so one doesn't need to include 2 namespaces to use the FileSystem
    [Flags]
    public enum FileShare
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write,
        Delete = 4,
        Inheritable = 16
    }

    // Copy of System.IO enum, so one doesn't need to include 2 namespaces to use the FileSystem
    public enum SearchOption
    {
        TopDirectoryOnly = 0,
        AllDirectories
    }

    public static class FileSystem
    {
        public static readonly char DirectorySeparatorChar = '/';
        public static readonly char AltDirectorySeparatorChar = '\\';
        public static readonly string DirectorySeparatorString = "/";
        public static readonly string AltDirectorySeparatorString = "\\";
        public static readonly char[] AllDirectorySeparatorChars = { DirectorySeparatorChar, AltDirectorySeparatorChar };

        private static readonly Dictionary<string, IFileProvider> Providers = new Dictionary<string, IFileProvider>();

        public static readonly IFileProvider User;
        public static IFileProvider Game;

        static FileSystem()
        {
            User = new FileProvider("/user", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private static bool ResolveProvider(string path, out IFileProvider provider, out string providerPath, bool isResolvingTop)
        {
            if (path.Contains(AltDirectorySeparatorChar))
            {
                path = path.Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);
            }
            for (var idx = path.Length - 1; idx >= 0; --idx)
            {
                var c = path[idx];
                var isResolvingTopC = idx == path.Length - 1 && isResolvingTop;
                if (!isResolvingTopC && c != DirectorySeparatorChar)
                {
                    continue;
                }
                providerPath = isResolvingTopC && c != DirectorySeparatorChar ? $"{path}{DirectorySeparatorChar}" : (idx + 1) == path.Length ? path : path.Substring(0, idx + 1);
                if (Providers.TryGetValue(providerPath, out provider))
                {
                    if (isResolvingTopC)
                    {
                        path = providerPath;
                    }
                    providerPath = path.Substring(providerPath.Length);
                    return true;
                }
            }
            provider = null;
            providerPath = null;
            return false;
        }

        private static int LastIndexOfDirectorySeparator(string path)
        {
            var length = path.Length;
            while (--length >= 0)
            {
                var c = path[length];
                if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar)
                {
                    return length;
                }
            }
            return -1;
        }

        private static int LastIndexOfDot(string path)
        {
            var length = path.Length;
            while (--length >= 0)
            {
                var c = path[length];
                if (c == '.')
                {
                    return length;
                }
            }
            return -1;
        }

        public static void RegisterProvider(IFileProvider provider)
        {
            if (provider.RootPath is null)
            {
                return;
            }
            if (Providers.ContainsKey(provider.RootPath))
            {
                throw new InvalidOperationException($"A provider with the root '{provider.RootPath}' is already registered.");
            }
            Providers.Add(provider.RootPath, provider);
        }

        public static void UnregisterProvider(IFileProvider provider)
        {
            var mountPoints = Providers.Where(x => x.Value == provider).ToArray();
            foreach (var mountPoint in mountPoints)
            {
                Providers.Remove(mountPoint.Key);
            }
        }

        public static bool HasProvider(string providerName)
        {
            foreach (var mountPoint in Providers.Keys)
            {
                if (string.Equals(providerName, mountPoint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static Stream OpenStream(string path, FileMode mode, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
        {
            if (ResolveProvider(path, out var provider, out var providerPath, false))
            {
                return provider.OpenStream(providerPath, mode, access, share);
            }
            return File.Open(path, (System.IO.FileMode) mode, (System.IO.FileAccess) access, (System.IO.FileShare) share);
        }

        public static string[] ListFiles(string path, string searchPattern, SearchOption searchOption)
        {
            if (ResolveProvider(path, out var provider, out var providerPath, true))
            {
                return provider.ListFiles(providerPath, searchPattern, searchOption).Select(x => provider.RootPath + x).ToArray();
            }
            return null;
        }

        public static async Task<string[]> ListFilesAsync(string path, string searchPattern, SearchOption searchOption)
        {
            if (ResolveProvider(path, out var provider, out var providerPath, true))
            {
                return await Task.Run(() => provider.ListFiles(providerPath, searchPattern, searchOption).Select(x => provider.RootPath + x).ToArray());
            }
            return null;
        }

        public static void CreateDirectory(string path)
        {
            if (ResolveProvider(path, out var provider, out var providerPath, true))
            {
                provider.CreateDirectory(providerPath);
            }
        }

        public static bool DirectoryExists(string path)
        {
            if (ResolveProvider(path, out var provider, out var providerPath, true))
            {
                return provider.DirectoryExists(providerPath);
            }
            return false;
        }

        public static bool FileExists(string path)
        {
            if (ResolveProvider(path, out var provider, out var providerPath, true))
            {
                return provider.FileExists(providerPath);
            }
            return false;
        }

        public static void FileDelete(string path)
        {
            if (ResolveProvider(path, out var provider, out var providerPath, true))
            {
                provider.FileDelete(providerPath);
            }
        }

        public static string Combine(string x, string y)
        {
            if (x.Length == 0)
            {
                return y;
            }
            if (y.Length == 0)
            {
                return x;
            }
            var lastX = x[x.Length - 1];
            var firstY = y[0];
            if (lastX != DirectorySeparatorChar && lastX != AltDirectorySeparatorChar)
            {
                if (firstY != DirectorySeparatorChar && firstY != AltDirectorySeparatorChar)
                {
                    return x + DirectorySeparatorChar + y;
                }
            }
            else if (firstY != DirectorySeparatorChar && firstY != AltDirectorySeparatorChar)
            {
                return x + y;
            }
            return x + y.Substring(1);
        }

        public static string Combine(params string[] args)
        {
            if (args.Length == 0)
            {
                return string.Empty;
            }
            var result = string.Empty;
            foreach (var str in args)
            {
                result = Combine(result, str);
            }
            return result;
        }

        public static string GetParentFolder(string path)
        {
            var lastSlashIdx = LastIndexOfDirectorySeparator(path);
            while (lastSlashIdx == path.Length - 1)
            {
                path = path.Substring(0, path.Length - 1);
                lastSlashIdx = LastIndexOfDirectorySeparator(path);
            }
            if (lastSlashIdx == -1)
            {
                throw new ArgumentException($"Path '{path}' does not contain a '/'.");
            }

            return path.Substring(0, lastSlashIdx);
        }

        public static string GetFileName(string path)
        {
            var lastSlashIdx = LastIndexOfDirectorySeparator(path);
            return path.Substring(lastSlashIdx + 1);
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            path = GetFileName(path);
            var lastDotIdx = LastIndexOfDot(path);
            if (lastDotIdx == -1)
            {
                return path;
            }
            return path.Substring(0, lastDotIdx);
        }
    }
}
