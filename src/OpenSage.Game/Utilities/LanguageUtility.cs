using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.IO;

namespace OpenSage.Utilities
{
    public enum GameLanguage
    {
        Chinese,
        Dutch,
        English,
        French,
        German,
        Italian,
        Norwegian,
        Polish,
        Spanish,
        Swedish
    }

    public static class LanguageUtility
    {
        private const GameLanguage DefaultLanguage = GameLanguage.English;

        public static GameLanguage ReadCurrentLanguage(IGameDefinition gameDefinition, FileSystem fileSystem)
        {
            switch (gameDefinition.Game)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.Bfme:
                    return DetectLanguage(gameDefinition, fileSystem);
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    return DetectLanguage(gameDefinition, fileSystem, "lang");
            }
            return DefaultLanguage;
        }

        private static GameLanguage DetectLanguage(IGameDefinition gameDefinition, FileSystem fileSystem, string langDirectory = "")
        {
            if (PlatformUtility.IsWindowsPlatform()
                && (gameDefinition.LanguageRegistryKeys != null && gameDefinition.LanguageRegistryKeys.Any())
                && ReadFromRegistry(gameDefinition.LanguageRegistryKeys, out var language))
            {
                return language;
            }

            foreach (GameLanguage lang in Enum.GetValues(typeof(GameLanguage)))
            {
                var languageFileExists = fileSystem
                    .GetFilesInDirectory(langDirectory).Any(x => x.FilePath.Contains(lang + ".big", StringComparison.InvariantCultureIgnoreCase));
                if (languageFileExists)
                {
                    return lang;
                }
            }
            return DefaultLanguage;
        }

        /// <summary>
        /// Used to read the installed language version from registry
        /// </summary>
        /// <param name="registryKeys"></param>
        /// <returns>language as string</returns>
        private static bool ReadFromRegistry(IEnumerable<RegistryKeyPath> registryKeys, out GameLanguage language)
        {
            language = DefaultLanguage;
            var registryValues = registryKeys.Select(RegistryUtility.GetRegistryValue);
            foreach (var registryValue in registryValues)
            {
                if (string.IsNullOrEmpty(registryValue))
                {
                    continue;
                }

                language = Enum.Parse<GameLanguage>(registryValue, true);
                return true;
            }

            return false;
        }
    }

}
