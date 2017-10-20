using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSage.Data
{
    public enum GameId
    {
        Generals,
        ZeroHour
    }

    public struct GameInstallation
    {
        public readonly GameId Game;
        public readonly string Path;

        public GameInstallation(GameId game, string path)
        {
            Game = game;
            Path = path;
        }

        public string DisplayName => GetDisplayName(Game);

        private static string GetDisplayName(GameId game)
        {
            switch (game)
            {
                case GameId.Generals: return "C&C Generals";
                case GameId.ZeroHour: return "C&C Generals Zero Hour";
            }

            throw new ArgumentException($"{game} is not supported.", nameof(game));
        }
    }

    public static class Games
    {
        public static IEnumerable<GameId> GetAll()
        {
            yield return GameId.Generals;
            yield return GameId.ZeroHour;
        }
    }

    public interface IInstallationLocator
    {
        IEnumerable<GameInstallation> FindInstallations(GameId game);
    }

    public class RegistryInstallationLocator : IInstallationLocator
    {
        private static readonly (string, string)[] GeneralsKeys = new[] { (@"SOFTWARE\Electronic Arts\EA Games\Generals", "InstallPath") };
        private static readonly (string, string)[] ZeroHourKeys = new[] { (@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "InstallPath") };

        private IEnumerable<(string keyName, string valueName)> GetRegistryKeysForGame(GameId game)
        {
            switch (game)
            {
                case GameId.Generals: return GeneralsKeys;
                case GameId.ZeroHour: return ZeroHourKeys;
                default: return Enumerable.Empty<(string, string)>();
            }
        }

        private string GetRegistryValue(string keyName, string valueName)
        {
            // 64-bit Windows uses a separate registry for 32-bit and 64-bit applications.
            // On a 64-bit system Registry.GetValue uses the 64-bit registry by default, which is why we have to read the value the "long way".
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = baseKey.OpenSubKey(keyName, false))
                {
                    var path = key.GetValue(valueName, null) as string;
                    if (path == null) return null;

                    return path;
                }
            }
        }

        public IEnumerable<GameInstallation> FindInstallations(GameId game)
        {
            var keys = GetRegistryKeysForGame(game);

            var installations = keys
                .Select(k => GetRegistryValue(k.keyName, k.valueName))
                .Where(Directory.Exists)
                .Select(p => new GameInstallation(game, p))
                .ToList();

            return installations;
        }
    }
}
