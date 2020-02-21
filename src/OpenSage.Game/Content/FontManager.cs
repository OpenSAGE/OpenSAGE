using System.Collections.Generic;
using System.Reflection;
using OpenSage.Gui;
using SixLabors.Fonts;

namespace OpenSage.Content
{
    public sealed class FontManager
    {
        private readonly Dictionary<FontKey, Font> _cachedFonts;

        // How Generals handles font fallback: https://github.com/OpenSAGE/OpenSAGE/issues/405
        private string FallbackUnicodeFont => "Arial Unicode MS";
        // According to what being seen in Generals, 
        // the "fallback fallback font" is Times New Roman
        private const string FallbackSystemFont = "Times New Roman";
        private const string FallbackEmbeddedFont = "Roboto";

        private readonly FontCollection _fallbackFonts;

        public FontManager()
        {
            _cachedFonts = new Dictionary<FontKey, Font>();

            _fallbackFonts = new FontCollection();

            var assembly = Assembly.GetExecutingAssembly();
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
                var embeddedFallback = false;

                if (!SystemFonts.TryFind(fontName, out _))
                {
                    // https://github.com/OpenSAGE/OpenSAGE/issues/405
                    // First try to load a unicode fallback font (e.g. Arial Unicode MS)
                    if (SystemFonts.TryFind(FallbackUnicodeFont, out _))
                    {
                        fontName = FallbackUnicodeFont;
                    }
                    // Then try to load a fallback system font (Arial)
                    else if (SystemFonts.TryFind(FallbackSystemFont, out _))
                    {
                        fontName = FallbackSystemFont;
                    }
                    else // If this fails use an embedded fallback font (Roboto)
                    {
                        embeddedFallback = true;
                    }
                }

                var fontStyle = fontWeight == FontWeight.Bold
                    ? FontStyle.Bold
                    : FontStyle.Regular;

                if (!embeddedFallback)
                {
                    font = SystemFonts.CreateFont(fontName, fontSize, fontStyle);
                }
                else
                {
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
