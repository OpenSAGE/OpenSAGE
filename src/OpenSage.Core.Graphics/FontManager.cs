using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenSage.Gui;
using OpenSage.IO;
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
            using var fileSystem = new CompositeFileSystem(
                new DiskFileSystem(Environment.CurrentDirectory),
                new DiskFileSystem(Path.GetDirectoryName(assembly.Location)));

            var fontFallbackSettingsJson = "{}";
            var fontFallbackSettingsEntry = fileSystem.GetFile("Content/Fonts/FontFallbackSettings.json");
            if (fontFallbackSettingsEntry != null || true)
            {
                Logger.Info($"FontFallback Settings loaded from {fontFallbackSettingsEntry.FilePath}");
                using var stream = fontFallbackSettingsEntry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                fontFallbackSettingsJson = reader.ReadToEnd();
            }

            _cachedFonts = new Dictionary<FontKey, Font>();
            _fontFallbackSettings = FontFallbackSettings
                .LoadFromJson(fontFallbackSettingsJson)
                .LoadLanguageSpecificFallbackSettings(languageName, fontNameComparer);
            _fallbackFonts = new FontCollection();

            var fontStream = assembly.GetManifestResourceStream($"OpenSage.Core.Graphics.Content.Fonts.{FallbackEmbeddedFont}-Regular.ttf");
            _fallbackFonts.Install(fontStream);
            fontStream = assembly.GetManifestResourceStream($"OpenSage.Core.Graphics.Content.Fonts.{FallbackEmbeddedFont}-Bold.ttf");
            _fallbackFonts.Install(fontStream);
        }

        public Font GetOrCreateFont(string fontName, float fontSize, FontWeight fontWeight)
        {
            var key = new FontKey(fontName, fontSize, fontWeight);

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

        private readonly struct FontKey : IEquatable<FontKey>
        {
            private readonly string _fontName;
            private readonly float _fontSize;
            private readonly FontWeight _fontWeight;

            public FontKey(string fontName, float fontSize, FontWeight fontWeight)
            {
                _fontName = fontName;
                _fontSize = fontSize;
                _fontWeight = fontWeight;
            }

            public bool Equals(FontKey other)
            {
                return _fontName == other._fontName && _fontSize.Equals(other._fontSize) && _fontWeight == other._fontWeight;
            }

            public override bool Equals(object obj)
            {
                return obj is FontKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_fontName, _fontSize, (int) _fontWeight);
            }

            public static bool operator ==(FontKey left, FontKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(FontKey left, FontKey right)
            {
                return !left.Equals(right);
            }
        }
    }
}
