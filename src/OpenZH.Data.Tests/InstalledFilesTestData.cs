using System;
using System.IO;
using System.Linq;
using OpenZH.Data.Big;
using Xunit.Abstractions;

namespace OpenZH.Data.Tests
{
    internal static class InstalledFilesTestData
    {
        public static void ReadFiles(string fileExtension, ITestOutputHelper output, Action<string, Func<Stream>> processFileCallback)
        {
            var rootDirectories = new[]
            {
                @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals",
                @"C:\Program Files (x86)\Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour"
            };

            var foundAtLeastOneFile = false;
            foreach (var directory in rootDirectories.Where(x => Directory.Exists(x)))
            {
                foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
                {
                    var ext = Path.GetExtension(file).ToLower();
                    if (ext == ".big")
                    {
                        output.WriteLine($"Reading BIG archive {Path.GetFileName(file)}.");

                        using (var bigStream = File.OpenRead(file))
                        using (var archive = new BigArchive(bigStream))
                        {
                            foreach (var entry in archive.Entries.Where(x => Path.GetExtension(x.FullName).ToLower() == fileExtension))
                            {
                                output.WriteLine($"Reading file {entry.FullName}.");

                                processFileCallback(entry.FullName, entry.Open);

                                foundAtLeastOneFile = true;
                            }
                        }
                    }
                    else if (ext == fileExtension)
                    {
                        output.WriteLine($"Reading file {file}.");

                        processFileCallback(file, () => File.OpenRead(file));

                        foundAtLeastOneFile = true;
                    }
                }
            }

            if (!foundAtLeastOneFile)
            {
                throw new Exception($"No files were found matching file extension {fileExtension}");
            }
        }
    }
}
