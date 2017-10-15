using System;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests
{
    internal static class InstalledFilesTestData
    {
        public static void ReadFiles(string fileExtension, ITestOutputHelper output, Action<FileSystemEntry> processFileCallback)
        {
            var rootDirectories = new[]
            {
                //@"C:\Program Files (x86)\EA Games\Command & Conquer The First Decade\Command & Conquer(tm) Generals",
                //@"C:\Program Files (x86)\EA Games\Command & Conquer The First Decade\Command & Conquer(tm) Generals Zero Hour",
                @"C:\Program Files (x86)\EA Games\The Battle for Middle-earth (tm)",
                //@"C:\Program Files (x86)\Electronic Arts\The Battle for Middle-earth (tm) II"
            };

            var foundAtLeastOneFile = false;
            foreach (var rootDirectory in rootDirectories.Where(x => Directory.Exists(x)))
            {
                var fileSystem = new FileSystem(rootDirectory);

                foreach (var file in fileSystem.Files)
                {
                    if (Path.GetExtension(file.FilePath).ToLower() != fileExtension)
                    {
                        continue;
                    }

                    output.WriteLine($"Reading file {file.FilePath}.");

                    processFileCallback(file);

                    foundAtLeastOneFile = true;
                }
            }

            if (!foundAtLeastOneFile)
            {
                throw new Exception($"No files were found matching file extension {fileExtension}");
            }
        }
    }
}
