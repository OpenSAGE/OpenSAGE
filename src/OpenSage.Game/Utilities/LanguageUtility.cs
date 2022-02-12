using System;
using System.IO;

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

        public static GameLanguage ReadCurrentLanguage(IGameDefinition gameDefinition, string rootDirectory)
        {
            switch (gameDefinition.Game)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.Bfme:
                    return DetectLanguage(rootDirectory);
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    return DetectLanguage(Path.Combine(rootDirectory, "lang"));
            }
            return DefaultLanguage;
        }

        private static GameLanguage DetectLanguage(string langDirectory)
        {
            foreach (GameLanguage lang in Enum.GetValues(typeof(GameLanguage)))
            {
                if (File.Exists(Path.Combine(langDirectory, lang + ".big")))
                {
                    return lang;
                }
            }
            return DefaultLanguage;
        }
    }

}
