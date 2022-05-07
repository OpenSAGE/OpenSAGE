using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data;
using OpenSage.IO;

namespace OpenSage.Utilities
{
    public static class LanguageUtility
    {
        private const string DefaultLanguage = "english";

        /// <summary>
        /// Detect the language for the current installation
        /// </summary>
        /// <param name="gameDefinition"></param>
        /// <param name="rootDirectory"></param>
        /// <returns>language as string</returns>
        public static string ReadCurrentLanguage(IGameDefinition gameDefinition, FileSystem fileSystem)
        {
            if (PlatformUtility.IsWindowsPlatform())
            {
                if (gameDefinition.LanguageRegistryKeys != null && gameDefinition.LanguageRegistryKeys.Any())
                {
                    if(ReadFromRegistry(gameDefinition.LanguageRegistryKeys, out var language))
                    {
                        return language;
                    }
                }
            }

            switch (gameDefinition.Game)
            {
                case SageGame.CncGenerals:
                    return DetectFromFileSystem(fileSystem, "", "Audio", ".big");
                case SageGame.CncGeneralsZeroHour:
                    return DetectFromFileSystem(fileSystem, "", "Audio", "ZH.big");
                case SageGame.Bfme:
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    return DetectFromFileSystem(fileSystem, "lang", "", "Audio.big");
                case SageGame.Ra3Uprising:
                case SageGame.Ra3:
                    return DetectFromFileSystem(fileSystem, "Data", "", "Audio.big");
            }

            return DefaultLanguage;
        }

        /// <summary>
        /// Used to read the installed language version from registry
        /// </summary>
        /// <param name="registryKeys"></param>
        /// <returns>language as string</returns>
        private static bool ReadFromRegistry(IEnumerable<RegistryKeyPath> registryKeys, out string language)
        {
            language = DefaultLanguage;
            var registryValues = registryKeys.Select(RegistryUtility.GetRegistryValue);
            foreach (var registryValue in registryValues)
            {
                if (!string.IsNullOrEmpty(registryValue))
                {
                    language = registryValue;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Used to identify the language based on the filesystem and an identifier file e.g. AudioEnglish.big
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <param name="filePrefix"></param>
        /// <param name="fileSuffix"></param>
        /// <returns>language as string</returns>
        private static string DetectFromFileSystem(FileSystem fileSystem, string directory, string filePrefix, string fileSuffix)
        {
            if (string.IsNullOrEmpty(filePrefix) && string.IsNullOrEmpty(fileSuffix))
            {
                return DefaultLanguage;
            }

            var files = fileSystem.GetFilesInDirectory(directory, $"{filePrefix}*{fileSuffix}") // there's no sense in searching subfolders
                .Select(x => Path.GetFileName(x.FilePath))
                .Select(x => string.IsNullOrEmpty(filePrefix) ? x : x[filePrefix.Length..])
                .Select(x => string.IsNullOrEmpty(fileSuffix) ? x : x[..^fileSuffix.Length]);
            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }
                return file;
            }
            return DefaultLanguage;
        }
    }
}
