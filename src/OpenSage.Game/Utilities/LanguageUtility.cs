using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenSage.Data;

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
        public static string ReadCurrentLanguage(IGameDefinition gameDefinition, string rootDirectory)
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
                    return DetectFromFileSystem(rootDirectory, "Audio", ".big");
                case SageGame.CncGeneralsZeroHour:
                    return DetectFromFileSystem(rootDirectory, "Audio", "ZH.big");
                case SageGame.Bfme:
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    return DetectFromFileSystem(rootDirectory, "", "Audio.big");
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
        private static string DetectFromFileSystem(string rootDirectory, string filePrefix, string fileSuffix)
        {
            if (string.IsNullOrEmpty(filePrefix) && string.IsNullOrEmpty(fileSuffix))
            {
                return DefaultLanguage;
            }

            var possibleFiles = Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories).Where(i =>
                    (string.IsNullOrEmpty(filePrefix) || Path.GetFileName(i).StartsWith(filePrefix)) &&
                    (string.IsNullOrEmpty(fileSuffix) || Path.GetFileName(i).EndsWith(fileSuffix)));

            if (string.IsNullOrEmpty(filePrefix))
            {
                foreach (var file in possibleFiles)
                {
                    if (file.EndsWith(fileSuffix))
                    {
                        return file.Replace(fileSuffix, "");
                    }
                }
            }
            else if (string.IsNullOrEmpty(fileSuffix))
            {
                foreach (var file in possibleFiles)
                {
                    if (file.StartsWith(filePrefix))
                    {
                        return file.Replace(filePrefix, "");
                    }
                }
            }
            else
            {
                foreach (var file in possibleFiles)
                {
                    MatchCollection mc = Regex.Matches(file, $"(?<={filePrefix})(.*)(?={fileSuffix})");
                    if (mc.Count > 0 && !string.IsNullOrEmpty(mc[0].Value))
                    {
                        return mc[0].Value;
                    }
                }
            }
            return DefaultLanguage;
        }
    }
}
