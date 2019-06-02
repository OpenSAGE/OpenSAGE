using System.IO;
using System.Collections.Generic;

namespace OpenSage.Tools.BigEditor.Util
{
    // Combinations files and directories together
    // in one enumerable object and provides a access via common interface
    public class Adapter
    {
        private readonly List<AdapterEntry> _entries;

        public readonly string RootDirectory;
        public IReadOnlyList<AdapterEntry> Entries => _entries;

        public Adapter(string path, string searchPattern = "*", AdapterFlags flags = AdapterFlags.None)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(path);
            DirectoryInfo[] directories = new DirectoryInfo[0];
            FileInfo[] files = new FileInfo[0];

            switch (flags)
            {
                case AdapterFlags.OnlyDirectories: {
                    directories = currentDirectory.GetDirectories();

                    break;
                }
                case AdapterFlags.OnlyFiles: {
                    files = currentDirectory.GetFiles(searchPattern);

                    break;
                }

                default: {
                    directories = currentDirectory.GetDirectories();
                    files = currentDirectory.GetFiles(searchPattern);

                    break;
                }
            }

            RootDirectory = currentDirectory.Parent.FullName;

            _entries = new List<AdapterEntry>(directories.Length + files.Length);

            foreach (var direcory in directories)
            {
                _entries.Add(new AdapterEntry(
                    $"{direcory.Name}/",
                    direcory.FullName,
                    "",
                    direcory.CreationTime,
                    direcory.Exists,
                    false,
                    0
                ));
            }

            foreach (var file in files)
            {
                _entries.Add(new AdapterEntry(
                    file.Name,
                    file.FullName,
                    file.Extension,
                    file.CreationTime,
                    file.Exists,
                    true,
                    file.Length
                ));
            }
        }
    }

    public enum AdapterFlags
    {
        None = 0,
        OnlyDirectories = 1,
        OnlyFiles = 2
    }
}