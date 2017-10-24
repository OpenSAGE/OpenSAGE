using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSage.Data
{
    public struct GameInstallation
    {
        public readonly SageGame Game;
        public readonly string Path;

        public GameInstallation(SageGame game, string path)
        {
            Game = game;
            Path = path;
        }

        public string DisplayName => GetDisplayName(Game);

        private static string GetDisplayName(SageGame game)
        {
            switch (game)
            {
                case SageGame.CncGenerals: return "C&C Generals";
                case SageGame.CncGeneralsZeroHour: return "C&C Generals Zero Hour";
            }

            throw new ArgumentException($"{game} is not supported.", nameof(game));
        }
    }

    public interface IInstallationLocator
    {
        IEnumerable<GameInstallation> FindInstallations(SageGame game);
    }

    public class RegistryInstallationLocator : IInstallationLocator
    {
        private static readonly (string, string)[] GeneralsKeys = { (@"SOFTWARE\Electronic Arts\EA Games\Generals", "InstallPath") };
        private static readonly (string, string)[] ZeroHourKeys = { (@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "InstallPath") };

        private static IEnumerable<(string keyName, string valueName)> GetRegistryKeysForGame(SageGame game)
        {
            switch (game)
            {
                case SageGame.CncGenerals: return GeneralsKeys;
                case SageGame.CncGeneralsZeroHour: return ZeroHourKeys;
                default: return Enumerable.Empty<(string, string)>();
            }
        }

        private static string GetRegistryValue(string keyName, string valueName)
        {
            // 64-bit Windows uses a separate registry for 32-bit and 64-bit applications.
            // On a 64-bit system Registry.GetValue uses the 64-bit registry by default, which is why we have to read the value the "long way".
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = baseKey.OpenSubKey(keyName, false))
                {
                    return key?.GetValue(valueName, null) as string;
                }
            }
        }

        public IEnumerable<GameInstallation> FindInstallations(SageGame game)
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
