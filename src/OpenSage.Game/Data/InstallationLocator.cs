using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public IGameDefinition Game { get; }
        public string Path { get; }

        private readonly GameInstallation _baseGameInstallation;

        public GameInstallation(IGameDefinition game, string path, GameInstallation baseGame = null)
        {
            Game = game;
            Path = path;
            _baseGameInstallation = baseGame;
        }

        public FileSystem CreateFileSystem()
        {
            FileSystem nextFileSystem = null;
            if (_baseGameInstallation != null)
            {
                nextFileSystem = new FileSystem(_baseGameInstallation.Path);
            }

            return new FileSystem(Path, nextFileSystem);
        }
    }

    public interface IInstallationLocator
    {
        IEnumerable<GameInstallation> FindInstallations(IGameDefinition game);
    }

    public class RegistryInstallationLocator : IInstallationLocator
    {
        private static string GetRegistryValue(RegistryKeyPath keyPath)
        {
            // 64-bit Windows uses a separate registry for 32-bit and 64-bit applications.
            // On a 64-bit system Registry.GetValue uses the 64-bit registry by default, which is why we have to read the value the "long way".
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = baseKey.OpenSubKey(keyPath.Key, false))
                {
                    var value = key?.GetValue(keyPath.ValueName, null) as string;

                    if (value != null && keyPath.Append != null)
                    {
                        value = Path.Combine(value, keyPath.Append);
                    }

                    return value;
                }
            }
        }

        // Validates paths to directories. Removes duplicates.
        private static IEnumerable<string> GetValidPaths(IEnumerable<string> paths)
        {
            return paths
                .WhereNot(string.IsNullOrWhiteSpace)
                .Distinct()
                .Where(Directory.Exists);
        }

        public IEnumerable<GameInstallation> FindInstallations(IGameDefinition game)
        {
            GameInstallation baseGameInstallation = null;

            if (game.BaseGame != null)
            {
                // TODO: Allow selecting one of these?
                baseGameInstallation = FindInstallations(game.BaseGame).FirstOrDefault();

                if (baseGameInstallation == null)
                {
                    // TODO: Log a warning / info message?
                    return Enumerable.Empty<GameInstallation>();
                }
            }

            var paths = game.RegistryKeys.Select(GetRegistryValue);

            var installations = GetValidPaths(paths)
                .Select(p => new GameInstallation(game, p, baseGameInstallation))
                .ToList();

            return installations;
        }
    }
}
