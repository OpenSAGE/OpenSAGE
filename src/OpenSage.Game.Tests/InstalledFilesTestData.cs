using System;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests
{
    internal static class InstalledFilesTestData
    {
        private static readonly IInstallationLocator locator;

        static InstalledFilesTestData()
        {
            locator = new RegistryInstallationLocator();
        }

        public static string GetInstallationDirectory(GameId game) => locator.FindInstallations(game).FirstOrDefault().Path;

        public static void ReadFiles(string fileExtension, ITestOutputHelper output, Action<FileSystemEntry> processFileCallback)
        {
            var rootDirectories = Games.GetAll().SelectMany(locator.FindInstallations).Select(i => i.Path);

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
