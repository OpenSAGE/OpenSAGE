using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenSage.Content
{
    // Handling fonts correctly in a cross-platform way could be quite complicated.
    // See https://github.com/OpenSAGE/OpenSAGE/issues/405
    // So for now, we specify code fonts fallback settings in a json file,
    // just to make things a bit easier.
    // In the future, we can also use this to handle font fallback:
    // https://github.com/SixLabors/Fonts/issues/117
    internal sealed class FontFallbackSettings
    {
        /// <summary>
        /// The default <see cref="LanguageSpecificFontFallbackSettings"/>.
        /// Other real language-specific settings always have priority. 
        /// </summary>
        public LanguageSpecificFontFallbackSettings DefaultSettings { get; set; } = new LanguageSpecificFontFallbackSettings
        {
            DefaultFallbacks = new[] {
                "Arial Unicode MS",
                "Arial",
                "Roboto"
            },
        };

        /// <summary>
        /// Define font fallbacks by languages.
        /// </summary>
        public Dictionary<string, LanguageSpecificFontFallbackSettings> FontFallbacksByLanguages { get; set; }

        private FontFallbackSettings() { }

        public static FontFallbackSettings LoadFromJson(string json)
        {
            var deserialized =
                JsonConvert.DeserializeObject<FontFallbackSettings>(json) ?? new FontFallbackSettings();
            deserialized.FontFallbacksByLanguages =
                NormalizeDictionary(deserialized.FontFallbacksByLanguages, StringComparer.OrdinalIgnoreCase);
            deserialized.DefaultSettings.Normalize(StringComparer.OrdinalIgnoreCase, null);

            return deserialized;
        }

        public LanguageSpecificFontFallbackSettings LoadLanguageSpecificFallbackSettings(string language, StringComparer comparer)
        {
            if (!FontFallbacksByLanguages.TryGetValue(language, out var result))
            {
                result = new LanguageSpecificFontFallbackSettings();
                FontFallbacksByLanguages[language] = result;
            }

            if (!result.Normalized)
            {
                result.Normalize(comparer, DefaultSettings);
            }

            return result;
        }

        /// <summary>
        /// Make <see cref="Dictionary{string, V}"/> case insensitive,
        /// and remove null entries.
        /// </summary>
        /// <typeparam name="V">The value type of <paramref name="dict"/></typeparam>
        /// <param name="dict">The dictionary to be normalized.</param>
        /// <param name="comparer">The <see cref="StringComparer"/> to be used.</param>
        /// <returns>The normalized <paramref name="dict"/></returns>
        public static Dictionary<string, V> NormalizeDictionary<V>(Dictionary<string, V>? dict, StringComparer comparer)
        {
            var nonNulls = dict?.Where(kv => kv.Value != null) ?? Enumerable.Empty<KeyValuePair<string, V>>();
            return new Dictionary<string, V>(nonNulls, comparer);
        }
    }

    internal sealed class LanguageSpecificFontFallbackSettings
    {
        /// <summary>
        /// The default fallback font families to be used for this language.
        /// They will be searched in the order they appear in the array.
        /// If none of the provided fallback fonts could be loaded,
        /// then <see cref="FontFallbackSettings.DefaultSettings"/> will be used.
        /// </summary>
        public string[] DefaultFallbacks { get; set; }

        /// <summary>
        /// For each hint font family name specified,
        /// provide an array of fallback font family names to be used.
        /// If none of the provided fallback fonts could be loaded,
        /// then <see cref="DefaultFallbacks"/> will be used.
        /// </summary>
        public Dictionary<string, string[]> SpecificFontFallbacks { get; set; }

        /// <summary>
        /// The default <see cref="LanguageSpecificFontFallbackSettings"/>,
        /// taken from parent <see cref="FontFallbackSettings"/>.
        /// </summary>
        private LanguageSpecificFontFallbackSettings _defaultSettings { get; set; }

        /// <summary>
        /// Will hold true if this <see cref="LanguageSpecificFontFallbackSettings"/>
        /// has already been "normalized", instead of being fresh raw values
        /// loaded from json.
        /// </summary>
        /// <param name="fontName"></param>
        [JsonIgnore]
        public bool Normalized { get; private set; } = false;

        /// <summary>
        /// Make <see cref="SpecificFontFallbacks"/> case insensitive,
        /// also load default fallbacks from parent settings.
        /// </summary>
        /// <param name="comparer">
        /// The case insensitive <see cref="StringComparer"/> to be used with
        /// <see cref="SpecificFontFallbacks"/>.
        /// </param>
        /// <param name="defaultSettings">
        /// The <see cref="LanguageSpecificFontFallbackSettings"/> of default language.
        /// </param>
        public void Normalize(StringComparer comparer, LanguageSpecificFontFallbackSettings? defaultSettings)
        {
            if (Normalized)
            {
                // already normalized
                throw new InvalidOperationException();
            }

            if (defaultSettings?.Normalized == false)
            {
                throw new InvalidOperationException();
            }

            SpecificFontFallbacks = FontFallbackSettings.NormalizeDictionary(SpecificFontFallbacks, comparer);
            DefaultFallbacks ??= new string[0];
            _defaultSettings = this != defaultSettings ? defaultSettings : null;

            Normalized = true;
        }

        public IEnumerable<string> GetFallbackList(string fontName)
        {
            if (!Normalized)
            {
                throw new InvalidOperationException();
            }

            var empty = Enumerable.Empty<string>();
            SpecificFontFallbacks.TryGetValue(fontName, out var fallbacks);
            string[] otherFallbacks = default;
            _defaultSettings?.SpecificFontFallbacks.TryGetValue(fontName, out otherFallbacks);
            return (fallbacks ?? empty)
                .Concat(otherFallbacks ?? empty)
                .Concat(DefaultFallbacks)
                .Concat(_defaultSettings?.DefaultFallbacks ?? empty);
        }
    }
}
