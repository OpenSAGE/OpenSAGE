using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data;
using OpenSage.IO;
using OpenSage.Mods.BuiltIn;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data
{
    internal static class InstalledFilesTestData
    {
        public static string GetInstallationDirectory(SageGame game)
        {
            var definition = GameDefinition.FromGame(game);
            return InstallationLocators.FindAllInstallations(definition).First().Path;
        }

        public static void ReadFiles(string fileExtension, ITestOutputHelper output, Action<FileSystemEntry> processFileCallback)
        {
            ReadFiles(fileExtension, output, GameDefinition.All, processFileCallback);
        }

        public static void ReadFiles(string fileExtension, ITestOutputHelper output, IEnumerable<IGameDefinition> gameDefinitions, Action<FileSystemEntry> processFileCallback)
        {
            var rootDirectories = gameDefinitions
                .SelectMany(InstallationLocators.FindAllInstallations)
                .Select(i => i.Path)
                .Where(x => Directory.Exists(x))
                .ToList();

            var foundAtLeastOneFile = false;

            foreach (var rootDirectory in rootDirectories)
            {
                using var fileSystem = new CompositeFileSystem(
                    new DiskFileSystem(rootDirectory),
                    new BigFileSystem(rootDirectory));

                foreach (var file in fileSystem.GetFilesInDirectory("", $"*.{fileExtension}", SearchOption.AllDirectories))
                {
                    output.WriteLine($"Reading file {file.FilePath}.");

                    processFileCallback(file);

                    foundAtLeastOneFile = true;
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
                .SelectMany(InstallationLocators.FindAllInstallations)
                .ToList();

            foreach (var installation in installations)
            {
                using (var game = new Game(installation, null))
                {
                    foreach (var file in game.ContentManager.FileSystem.GetFilesInDirectory("", $".{fileExtension}", SearchOption.AllDirectories))
                    {
                        output.WriteLine($"Reading file {file.FilePath}.");

                        processFileCallback(game, file);

                        foundAtLeastOneFile = true;
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
