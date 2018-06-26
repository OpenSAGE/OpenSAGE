using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Utilities;

namespace OpenSage
{
    public static class LanguageSetting
    {
        //do we need a default?
        private const string DefaultLanguage = "englisch";
        private static string _detectedLanguage;

        public static string Current => string.IsNullOrEmpty(_detectedLanguage) ? DefaultLanguage : _detectedLanguage;

        public static void ReadFromRegistry(IGameDefinition gameDefinition)
        {
            var languageRegistryKey = gameDefinition.RegistryKeys.FirstOrDefault(i => i.ValueName == "Language");
            if (languageRegistryKey != null)
            {
                _detectedLanguage = RegistryReader.GetRegistryValue(languageRegistryKey);
            }
        }
    }
}
