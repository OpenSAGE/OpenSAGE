using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.IO;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Data
{
    public sealed class RegistryKeyPath
    {
        public readonly string Key;
        public readonly string ValueName;

        // This is required because one possible registry key for the Generals + ZH bundle points to the
        // root directory of the bundle.
        public readonly string Append;

        public RegistryKeyPath(string key, string valueName, string append = null)
        {
            Key = key;
            ValueName = valueName;
            Append = append;
        }
    }

    public sealed class GameInstallation
    {
        public static IEnumerable<GameInstallation> FindAll(IEnumerable<IGameDefinition> gameDefinitions)
        {
            return InstallationLocators
                .GetAllForPlatform()
                .SelectMany(x => gameDefinitions.SelectMany(y => x.FindInstallations(y)));
        }

        public IGameDefinition Game { get; }
        public string Path { get; }

        private readonly GameInstallation _baseGameInstallation;

        public GameInstallation(IGameDefinition game, string path, GameInstallation baseGame = null)
        {
            Game = game;
            Path = path;
            _baseGameInstallation = baseGame;
        }

        public IFileProvider CreateFileSystem()
        {
            var result = new FileProvider("/game", Path);
            var bigFiles = result.ListFiles("", "*.big", IO.SearchOption.AllDirectories);
            foreach (string bigFile in bigFiles)
            {
                result.AddSubProvider(new BigProvider(null, new FileFormats.Big.BigArchive(bigFile, result.OpenStream(bigFile, IO.FileMode.Open))));
            }
            if (_baseGameInstallation != null)
            {
                var subResult = new FileProvider(null, _baseGameInstallation.Path);
                var subBigFiles = subResult.ListFiles("", "*.big", IO.SearchOption.AllDirectories);
                foreach (string bigFile in subBigFiles)
                {
                    subResult.AddSubProvider(new BigProvider(null, new FileFormats.Big.BigArchive(bigFile, subResult.OpenStream(bigFile, IO.FileMode.Open))));
                }
                result.AddSubProvider(subResult);
            }
            return result;
        }
    }

    public interface IInstallationLocator
    {
        IEnumerable<GameInstallation> FindInstallations(IGameDefinition game);
    }

    public class EnvironmentInstallationLocator : IInstallationLocator
    {
        public IEnumerable<GameInstallation> FindInstallations(IGameDefinition game)
        {
            var identifier = game.Identifier.ToUpperInvariant() + "_PATH";
            var path = Environment.GetEnvironmentVariable(identifier);

            if (path == null)
            {
                path = Environment.GetEnvironmentVariable(identifier, EnvironmentVariableTarget.User);
            }
            if (path == null || !Directory.Exists(path))
            {
                return new GameInstallation[] { };
            }

            var installations = new GameInstallation[] { new GameInstallation(game, path) };

            return installations;
        }
    }

    // TODO: Move this to the Platform project.
    public class RegistryInstallationLocator : IInstallationLocator
    {

        // Validates paths to directories. Removes duplicates.
        private static IEnumerable<string> GetValidPaths(IEnumerable<string> paths)
        {
            return paths
                .WhereNot(string.IsNullOrWhiteSpace)
                .Distinct()
                .Where(Directory.Exists);
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IEnumerable<GameInstallation> FindInstallations(IGameDefinition game)
        {
            GameInstallation baseGameInstallation = null;

            if (game.BaseGame != null)
            {
                // TODO: Allow selecting one of these?
                baseGameInstallation = FindInstallations(game.BaseGame).FirstOrDefault();

                if (baseGameInstallation == null)
                {
                    logger.Warn("No game installations found");
                    return Enumerable.Empty<GameInstallation>();
                }
            }

            var paths = game.RegistryKeys.Select(RegistryUtility.GetRegistryValue);

            var installations = GetValidPaths(paths)
                .Select(p => new GameInstallation(game, p, baseGameInstallation))
                .ToList();

            return installations;
        }
    }

    public static class InstallationLocators
    {
        public static IEnumerable<IInstallationLocator> GetAllForPlatform()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                yield return new RegistryInstallationLocator();
            }

            yield return new EnvironmentInstallationLocator();
        }

        public static IEnumerable<GameInstallation> FindAllInstallations(IGameDefinition game)
        {
            var locators = GetAllForPlatform();
            var result = new List<GameInstallation>();
            foreach (var locator in locators)
            {
                var installations = locator.FindInstallations(game);
                foreach (var installation in installations)
                {
                    if (!result.Contains(installation))
                    {
                        result.Add(installation);
                    }
                }
            }
            return result;
        }
    }
}
