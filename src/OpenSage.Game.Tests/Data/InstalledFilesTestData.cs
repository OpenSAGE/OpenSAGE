using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data
{
    internal static class InstalledFilesTestData
    {
        private static readonly IInstallationLocator Locator;

        static InstalledFilesTestData()
        {
            Locator = new RegistryInstallationLocator();
        }

        public static string GetInstallationDirectory(SageGame game)
        {
            var definition = GameDefinition.FromGame(game);
            return Locator.FindInstallations(definition).First().Path;
        }

        public static void ReadFiles(string fileExtension, ITestOutputHelper output, Action<FileSystemEntry> processFileCallback)
        {
            ReadFiles(fileExtension, output, GameDefinition.All, processFileCallback);
        }

        public static void ReadFiles(string fileExtension, ITestOutputHelper output, IEnumerable<IGameDefinition> gameDefinitions, Action<FileSystemEntry> processFileCallback)
        {
            var rootDirectories = gameDefinitions
                .SelectMany(Locator.FindInstallations)
                .Select(i => i.Path)
                .Where(x => Directory.Exists(x))
                .ToList();

            var foundAtLeastOneFile = false;
            foreach (var rootDirectory in rootDirectories)
            {
                using (var fileSystem = new FileSystem(rootDirectory))
                {
                    foreach (var file in fileSystem.Files)
                    {
                        if (Path.GetExtension(file.FilePath).ToLowerInvariant() != fileExtension)
                        {
                            continue;
                        }

                        output.WriteLine($"Reading file {file.FilePath}.");

                        processFileCallback(file);

                        foundAtLeastOneFile = true;
                    }
                }
            }

            if (rootDirectories.Count > 0 && !foundAtLeastOneFile)
            {
                throw new Exception($"No files were found matching file extension {fileExtension}");
            }
        }

        public static void ReadFiles(string fileExtension, ITestOutputHelper output, Action<Game, FileSystemEntry> processFileCallback)
        {
            ReadFiles(fileExtension, output, GameDefinition.All, processFileCallback);
        }

        public static void ReadFiles(string fileExtension, ITestOutputHelper output, IEnumerable<IGameDefinition> gameDefinitions, Action<Game, FileSystemEntry> processFileCallback)
        {
            var foundAtLeastOneFile = false;

            var installations = gameDefinitions
                .SelectMany(Locator.FindInstallations)
                .ToList();

            using (var window = new GameWindow("Test", 10, 10, 100, 100, null))
            using (var panel = GamePanel.FromGameWindow(window))
            {
                foreach (var installation in installations)
                {
                    using (var fileSystem = installation.CreateFileSystem())
                    using (var game = GameFactory.CreateGame(installation, fileSystem, panel))
                    {
                        foreach (var file in fileSystem.Files)
                        {
                            if (Path.GetExtension(file.FilePath).ToLowerInvariant() != fileExtension)
                            {
                                continue;
                            }

                            output.WriteLine($"Reading file {file.FilePath}.");

                            processFileCallback(game, file);

                            foundAtLeastOneFile = true;
                        }
                    }
                }
            }

            if (installations.Count > 0 && !foundAtLeastOneFile)
            {
                throw new Exception($"No files were found matching file extension {fileExtension}");
            }
        }
    }
}
