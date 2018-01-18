using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Data
{
    public sealed class GameInstallation
    {
        public SageGame Game { get; }
        public string Path { get; }
        public string SecondaryPath { get; }

        public string DisplayName
        {
            get
            {
                switch (Game)
                {
                    case SageGame.CncGenerals:
                        return "C&C Generals";

                    case SageGame.CncGeneralsZeroHour:
                        return "C&C Generals Zero Hour";

                    case SageGame.BattleForMiddleEarth:
                        return "Battle for Middle-earth";

                    case SageGame.BattleForMiddleEarthII:
                        return "Battle for Middle-earth II";

                    case SageGame.Cnc3:
                        return "C&C 3: Tiberium Wars";

                    case SageGame.Cnc3KanesWrath:
                        return "C&C 3: Kane's Wrath";

                    case SageGame.Ra3:
                        return "C&C: Red Alert 3";

                    case SageGame.Ra3Uprising:
                        return "C&C: Red Alert 3 Uprising";

                    case SageGame.Cnc4:
                        return "C&C 4";

                    default:
                        throw new InvalidOperationException($"{Game} is not supported.");
                }
            }
        }

        public string LauncherImagePath
        {
            get
            {
                switch (Game)
                {
                    case SageGame.CncGenerals:
                    case SageGame.CncGeneralsZeroHour:
                        return @"Install_Final.bmp";

                    // TODO: Needs to be localised?
                    case SageGame.BattleForMiddleEarth:
                        return "englishsplash.jpg";

                    // TODO: Needs to be localised?
                    case SageGame.BattleForMiddleEarthII:
                        return "EnglishSplash.jpg";

                    case SageGame.Cnc3:
                    case SageGame.Cnc3KanesWrath:
                    case SageGame.Ra3:
                    case SageGame.Ra3Uprising:
                        return @"Launcher\splash.bmp";

                    // TODO: Don't think there is a standalone splash image.
                    // Probably part of the launcher that shows the EA login screen.
                    case SageGame.Cnc4:
                        return null;

                    default:
                        throw new InvalidOperationException($"{Game} is not supported.");
                }
            }
        }

        public GameInstallation(SageGame game, string path, string secondaryPath = null)
        {
            Game = game;
            Path = path;
            SecondaryPath = secondaryPath;
        }

        public FileSystem CreateFileSystem()
        {
            FileSystem nextFileSystem = null;
            if (SecondaryPath != null)
            {
                nextFileSystem = new FileSystem(SecondaryPath);
            }

            return new FileSystem(Path, nextFileSystem);
        }
    }

    public interface IInstallationLocator
    {
        IEnumerable<GameInstallation> FindInstallations(SageGame game);
    }

    public class RegistryInstallationLocator : IInstallationLocator
    {
        private static readonly (string, string)[] GeneralsKeys = { (@"SOFTWARE\Electronic Arts\EA Games\Generals", "InstallPath") };

        private static readonly (string, string)[] ZeroHourKeys =
        {
            (@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer The First Decade", "zh_folder"),
            (@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "InstallPath")
        };
        
        private static readonly (string, string)[] BfmeKeys = { (@"SOFTWARE\Electronic Arts\EA Games\The Battle for Middle-earth", "InstallPath") };
        private static readonly (string, string)[] BfmeIIKeys = { (@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "InstallPath") };

        private static readonly (string, string)[] Cnc3Keys = { (@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3", "InstallPath") };
        private static readonly (string, string)[] Cnc3KanesWrathKeys = { (@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3 Kanes Wrath", "InstallPath") };

        private static readonly (string, string)[] Ra3Keys = { (@"SOFTWARE\Electronic Arts\Electronic Arts\Red Alert 3", "Install Dir") };
        private static readonly (string, string)[] Ra3UprisingKeys = { (@"SOFTWARE\Electronic Arts\Electronic Arts\Red Alert 3 Uprising", "Install Dir") };

        private static readonly (string, string)[] Cnc4Keys =
            {
                (@"SOFTWARE\EA Games\Command Conquer 4 Tiberian Twilight", "Install Dir"), // Origin
                (@"SOFTWARE\Electronic Arts\command and conquer 4", "install dir") // Steam
            };

        private static IEnumerable<(string keyName, string valueName)> GetRegistryKeysForGame(SageGame game)
        {
            switch (game)
            {
                case SageGame.CncGenerals:
                    return GeneralsKeys;

                case SageGame.CncGeneralsZeroHour:
                    return ZeroHourKeys;

                case SageGame.BattleForMiddleEarth:
                    return BfmeKeys;

                case SageGame.BattleForMiddleEarthII:
                    return BfmeIIKeys;

                case SageGame.Cnc3:
                    return Cnc3Keys;

                case SageGame.Cnc3KanesWrath:
                    return Cnc3KanesWrathKeys;

                case SageGame.Ra3:
                    return Ra3Keys;

                case SageGame.Ra3Uprising:
                    return Ra3UprisingKeys;

                case SageGame.Cnc4:
                    return Cnc4Keys;

                default:
                    return Enumerable.Empty<(string, string)>();
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

        // Validates paths to directories. Removes duplicates.
        private IEnumerable<string> GetValidPaths(IEnumerable<string> paths)
        {
            return paths
                .WhereNot(string.IsNullOrWhiteSpace)
                .Distinct()
                .Where(Directory.Exists);
        }

        // Zero Hour requires some special handling compared to any other game.
        // For starters it is an expansion pack, so it requires an installation of the base game.
        // It also sometimes uses a registry key which doesn't point directly to the installation directory.
        private IEnumerable<GameInstallation> FindZeroHourInstallations()
        {
            var generalsPath = FindInstallations(SageGame.CncGenerals).FirstOrDefault()?.Path;

            // If there's no installation of Generals there's no reason to bother looking for Zero Hour.
            if (generalsPath == null)
            {
                return Enumerable.Empty<GameInstallation>();
            }

            var keys = GetRegistryKeysForGame(SageGame.CncGeneralsZeroHour);

            // For an unknown reason sometimes the Origin version gets installed with a different registry key.
            // This wouldn't be an issue, except for the fact it points to the root of the Generals bundle, not to the Zero Hour expansion itself.
            var originVersionRootPath = GetRegistryValue(@"SOFTWARE\EA Games\Command and Conquer Generals Zero Hour", "Install Dir");
            var originVersionPath = originVersionRootPath == null ? null : Path.Combine(originVersionRootPath, "Command and Conquer Generals Zero Hour\\");

            var paths = new List<string>();
            paths.AddRange(keys.Select(k => GetRegistryValue(k.keyName, k.valueName)));
            paths.Add(originVersionPath);

            var installations = GetValidPaths(paths)
                .Select(p => new GameInstallation(SageGame.CncGeneralsZeroHour, p, generalsPath))
                .ToList();

            return installations;
        }

        public IEnumerable<GameInstallation> FindInstallations(SageGame game)
        {
            if (game == SageGame.CncGeneralsZeroHour)
            {
                return FindZeroHourInstallations();
            }

            var keys = GetRegistryKeysForGame(game);
            var paths = keys.Select(k => GetRegistryValue(k.keyName, k.valueName));

            var installations = GetValidPaths(paths)
                .Select(p => new GameInstallation(game, p))
                .ToList();

            return installations;
        }
    }
}
