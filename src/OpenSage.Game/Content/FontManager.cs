using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenSage.Data;
using OpenSage.Gui;
using SixLabors.Fonts;

namespace OpenSage.Content
{
    public sealed class FontManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Dictionary<FontKey, Font> _cachedFonts;

        // See https://github.com/OpenSAGE/OpenSAGE/issues/405
        private readonly LanguageSpecificFontFallbackSettings _fontFallbackSettings;

        private const string FallbackEmbeddedFont = "Roboto";

        private readonly FontCollection _fallbackFonts;

        public FontManager(string languageName, StringComparer fontNameComparer)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // For the json file, I think it's better to leave it on the disk
            // (instead of embedding it) so users can edit it
            using var assemblyFileSystem = new FileSystem(Path.GetDirectoryName(assembly.Location));
            using var fileSystem = new FileSystem(Environment.CurrentDirectory, assemblyFileSystem);

            var fontFallbackSettingsJson = "{}";
            var fontFallbackSettingsEntry = fileSystem.GetFile("UserSettingsFiles/FontFallbackSettings.json");
            if (fontFallbackSettingsEntry != null || true)
            {
                Logger.Info($"FontFallback Settings loaded from {fontFallbackSettingsEntry.FullFilePath}");
                using var stream = fontFallbackSettingsEntry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                fontFallbackSettingsJson = reader.ReadToEnd();
            }

            _cachedFonts = new Dictionary<FontKey, Font>();
            _fontFallbackSettings = FontFallbackSettings
                .LoadFromJson(fontFallbackSettingsJson)
                .LoadLanguageSpecificFallbackSettings(languageName, fontNameComparer);
            _fallbackFonts = new FontCollection();


            var fontStream = assembly.GetManifestResourceStream($"OpenSage.Content.Fonts.{FallbackEmbeddedFont}-Regular.ttf");
            _fallbackFonts.Install(fontStream);
            fontStream = assembly.GetManifestResourceStream($"OpenSage.Content.Fonts.{FallbackEmbeddedFont}-Bold.ttf");
            _fallbackFonts.Install(fontStream);
        }

        public Font GetOrCreateFont(string fontName, float fontSize, FontWeight fontWeight)
        {
            var key = new FontKey
            {
                FontName = fontName,
                FontSize = fontSize,
                FontWeight = fontWeight
            };

            if (!_cachedFonts.TryGetValue(key, out var font))
            {
                var alternatives = _fontFallbackSettings.GetFallbackList(fontName);
                var fontNameFound = alternatives
                    .Prepend(fontName)
                    .FirstOrDefault(name => SystemFonts.TryFind(name, out _));
                if (fontNameFound != fontName)
                {
                    Logger.Info($"Requesting font {fontName}, actually found {fontNameFound}");
                }

                var fontStyle = fontWeight == FontWeight.Bold
                    ? FontStyle.Bold
                    : FontStyle.Regular;

                if (fontNameFound != null)
                {
                    font = SystemFonts.CreateFont(fontNameFound, fontSize, fontStyle);
                }
                else
                {
                    Logger.Info($"Will use embedded font as fallback for font {fontName}");
                    font = _fallbackFonts.CreateFont(FallbackEmbeddedFont, fontSize, fontStyle);
                }
                _cachedFonts.Add(key, font);
            }

            return font;
        }

        private struct FontKey
        {
            public string FontName;
            public float FontSize;
            public FontWeight FontWeight;
        }
    }
}
