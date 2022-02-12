using System;
using System.Linq;
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
                    return DetectLanguage(fileSystem, "");
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    return DetectLanguage(fileSystem, "lang");
            }
            return DefaultLanguage;
        }

        private static GameLanguage DetectLanguage(FileSystem fileSystem, string langDirectory = "")
        {
            foreach (GameLanguage lang in Enum.GetValues(typeof(GameLanguage)))
            {
                var languageFileExists = fileSystem
                    .GetFilesInDirectory(langDirectory).Any(x => x.FilePath.Contains(lang + ".big"));
                if (languageFileExists)
                {
                    return lang;
                }
            }
            return DefaultLanguage;
        }
    }

}
