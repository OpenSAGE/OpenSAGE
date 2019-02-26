using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenSage.FileFormats;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Content.Translation.Providers
{
    public sealed class CsfTranslationProvider : ATranslationProviderBase
    {
        private enum LanguageGenerals
        {
            [Display("en-US")] EnglishUS,
            [Display("en-UK")] EnglishUK,
            [Display("de-DE")] German,
            [Display("fr-FR")] French,
            [Display("es-ES")] Spanish,
            [Display("it-IT")] Italian,
            [Display("ja-JP")] Japanese,
            [Display("en-US")] Jabber,
            [Display("ko-KR")] Korean,
            [Display("zh-CN")] ChineseSimplified,
            [Display("pl-PL")] Polish = 12,
        }
        private enum LanguageBFME
        {
            [Display("en-US")] English,
            [Display("es-ES")] Spanish,
            [Display("de-DE")] German = 3,
            [Display("fr-FR")] French,
            [Display("it-IT")] Italian,
            [Display("nl-NL")] Dutch,
            [Display("pl-PL")] Polish = 11,
            [Display("nb-NO")] Norwegan,
            [Display("zh-HK")] ChineseTraditional,
            [Display("ru-RU")] Russian = 17
        }
        private enum LanguageCnC3
        {
            [Display("en-US")] English,
            [Display("es-ES")] Spanish,
            [Display("de-DE")] German = 3,
            [Display("fr-FR")] French,
            [Display("it-IT")] Italian,
            [Display("nl-NL")] Dutch,
            [Display("sv-SE")] Swedish = 8,
            [Display("pl-PL")] Polish = 11,
            [Display("ko-KR")] Korean = 13,
            [Display("hu-HU")] Hungarian = 15,
            [Display("ru-RU")] Russian = 17,
            [Display("cs-CZ")] Czech = 19,
            [Display("zh-CN")] ChineseSimplified = 21,
            [Display("zh-HK")] ChineseTraditional
        }
        // Note: CnC3 only uses language code for the launcher's csf
        // Note: RA3 and CnC4 doesn't use language codes at all

        private class Csf
        {
            private const int _magicCsf = 0x43534620;
            private const int _magicLabel = 0x4C424C20;
            private const int _magicLString = 0x53545220;
            private const int _magicLWideString = 0x53545257;

            internal readonly Dictionary<string, Dictionary<string, string>> _strings;

            internal int _numStrings;
            internal string _language;

            public Csf()
            {
                _strings = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            }

            public static void ReadCsf(Csf csf, Stream stream, SageGame game)
            {
                using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    var magic = reader.ReadInt32();
                    if (magic != _magicCsf)
                    {
                        throw new InvalidDataException("Error parsing csf (Magic: CSF expected).");
                    }
                    var version = reader.ReadInt32();
                    var numLabels = reader.ReadInt32();
                    var numStrings = reader.ReadInt32();
                    if (numLabels != numStrings)
                    {
                        // throw new NotSupportedException("Csf substrings are not supported.");
                        Debug.WriteLine("[CSF] Number of labels and strings mismatch.");
                    }
                    csf._numStrings = numStrings;
                    reader.ReadUInt32(); // reserved
                    var languageCode = reader.ReadUInt32();
                    string language;
                    switch (game)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour:
                            language = ((LanguageGenerals) languageCode).GetName();
                            break;
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            language = ((LanguageBFME) languageCode).GetName();
                            break;
                        case SageGame.Cnc3:
                        case SageGame.Cnc3KanesWrath:
                            language = ((LanguageCnC3) languageCode).GetName();
                            break;
                        default:
                            language = "en-US";
                            break;
                    }
                    if (language is null)
                    {
                        language = "en-US";
                        Console.WriteLine($"Unknown language id {languageCode} for game {game}.");
                    }
                    csf._language = language;
                    string label;
                    int strCount;
                    string str;
                    int colonIdx;
                    string categoryLabel;
                    for (var idx = 0; idx < numLabels; ++idx)
                    {
                        magic = reader.ReadInt32();
                        if (magic != _magicLabel)
                        {
                            throw new InvalidDataException("Error parsing csf (Magic: LBL expected).");
                        }
                        strCount = reader.ReadInt32();
                        label = reader.ReadUInt32PrefixedAsciiString();
                        for (var idy = 0; idy < strCount; ++idy)
                        {
                            magic = reader.ReadInt32();
                            if (magic != _magicLString && magic != _magicLWideString)
                            {
                                throw new InvalidDataException("Error parsing csf (Magic: STR/STRW expected).");
                            }
                            if (idy == 0)
                            {
                                str = reader.ReadUInt32PrefixedNegatedUnicodeString();
                                if (magic == _magicLWideString)
                                {
                                    str += reader.ReadUInt32PrefixedAsciiString();
                                }
                                colonIdx = label.IndexOf(':');
                                if (colonIdx == -1)
                                {
                                    categoryLabel = string.Empty;
                                    Console.WriteLine($"Empty category found for {label}.");
                                }
                                else
                                {
                                    categoryLabel = label.Substring(0, colonIdx).ToUpperInvariant();
                                }
                                if (!csf._strings.TryGetValue(categoryLabel, out var category))
                                {
                                    category = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    csf._strings.Add(categoryLabel, category);
                                }
                                label = label.Substring(colonIdx + 1);
                                if (category.ContainsKey(label))
                                {
                                    Console.WriteLine($"[CSF] String duplication: '{categoryLabel}:{label}' -> '{category[label]}', new value: '{str}'");
                                }
                                else
                                {
                                    category.Add(label, str);
                                }
                            }
                            else
                            {
                                if (magic == _magicLString)
                                {
                                    Console.WriteLine($"[CSF] Skipping substring '{reader.ReadUInt32PrefixedNegatedUnicodeString()}' from {label}.");
                                }
                                else
                                {
                                    Console.WriteLine($"[CSF] Skipping substring '{reader.ReadUInt32PrefixedNegatedUnicodeString()}{reader.ReadUInt32PrefixedAsciiString()}' from {label}.");
                                }
                            }
                        }
                    }
                }
            }

            public bool TryGetString(string str, out string result)
            {
                var colonIdx = str.IndexOf(':');
                var label = string.Empty;
                if (colonIdx == -1)
                {
                    result = str;
                    return false;
                }
                label = str.Substring(0, colonIdx);
                if (_strings.TryGetValue(label, out var category) && category.TryGetValue(str.Substring(colonIdx + 1), out result))
                {
                    return true;
                }
                result = null;
                return false;
            }
        }

        private Csf _csf;

        public override string Name => NameOverride ?? _csf._language;
        public override IReadOnlyCollection<string> Labels
        {
            get
            {
                var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var label in _csf._strings)
                {
                    foreach (var str in label.Value)
                    {
                        result.Add($"{label.Key}:{str.Key}");
                    }
                }
                return result;
            }
        }

        public CsfTranslationProvider(Stream stream, SageGame game)
        {
            Debug.Assert(!(stream is null), $"{nameof(stream)} is null");
            _csf = new Csf();
            Csf.ReadCsf(_csf, stream, game);
        }

        public override string GetString(string str)
        {
            Debug.Assert(!(str is null), $"{nameof(str)} is null");
            if (!_csf.TryGetString(str, out var result))
            {
                Console.WriteLine($"Requested string '{str}' not found in '{Name}'.");
            }
            return result;
        }

        public override string ToString()
        {
            return $"[CSF: {Name} - {_csf._numStrings} strings in {_csf._strings.Count} categories]";
        }
    }
}
