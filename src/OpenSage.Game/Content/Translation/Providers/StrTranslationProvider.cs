using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OpenSage.Content.Translation.Providers
{
    public sealed class StrTranslationProvider : ATranslationProviderBase
    {
        private class Str
        {
            private enum State
            {
                Begin,
                CommentBegin,
                Category,
                Label,
                PreValue,
                CommentPreValue,
                Value,
                String,
                CommentPreEnd,
                End1,
                End2,
                End3
            }

            internal readonly Dictionary<string, Dictionary<string, string>> _strings;

            internal int _numStrings;
            internal string _language;

            public Str()
            {
                _strings = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            }


            private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            public static void ReadStr(Str str, Stream stream, string language)
            {
                str._language = language;
                var category = string.Empty;
                var label = string.Empty;
                var value = string.Empty;
                var sb = new StringBuilder();
                var state = State.Begin;
                char c;
                var reader = new BinaryReader(stream, Encoding.ASCII);
                var isEscaped = false;
                while (stream.Position < stream.Length)
                {
                    c = reader.ReadChar();
                    switch (state)
                    {
                        case State.CommentBegin:
                            if (c == '\n' || c == '\r')
                            {
                                state = State.Begin;
                            }
                            break;
                        case State.CommentPreValue:
                            if (c == '\n' || c == '\r')
                            {
                                state = State.PreValue;
                            }
                            break;
                        case State.CommentPreEnd:
                            if (c == '\n' || c == '\r')
                            {
                                state = State.End1;
                            }
                            break;
                        case State.Begin:
                            if (char.IsWhiteSpace(c))
                            {
                                continue;
                            }
                            else if (c == '/')
                            {
                                c = reader.ReadChar();
                                if (c == '/')
                                {
                                    state = State.CommentBegin;
                                }
                            }
                            else if (c == ';')
                            {
                                state = State.CommentBegin;
                            }
                            // TODO: multiline comments
                            else if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                            {
                                state = State.Category;
                                sb.Append(c);
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected token {c}.");
                            }
                            break;
                        case State.Category:
                            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                            {
                                sb.Append(c);
                            }
                            else if (c == ':')
                            {
                                category = sb.ToString();
                                sb.Clear();
                                state = State.Label;
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected token {c}.");
                            }
                            break;
                        case State.Label:
                            if (c >= '!' && c <= 'z')
                            {
                                sb.Append(c);
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                label = sb.ToString();
                                sb.Clear();
                                state = State.PreValue;
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected token {c}.");
                            }
                            break;
                        case State.PreValue:
                            if (char.IsWhiteSpace(c))
                            {
                                continue;
                            }
                            else if (c == '/')
                            {
                                c = reader.ReadChar();
                                if (c == '/')
                                {
                                    state = State.CommentPreValue;
                                }
                            }
                            else if (c == ';')
                            {
                                state = State.CommentPreValue;
                            }
                            // TODO: multiline comments
                            else if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                            {
                                state = State.Value;
                                sb.Append(c);
                            }
                            else if (c == '"')
                            {
                                state = State.String;
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected token {c}.");
                            }
                            break;
                        case State.Value:
                            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                            {
                                sb.Append(c);
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                value = sb.ToString();
                                sb.Clear();
                                state = State.End1;
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected token {c}.");
                            }
                            break;
                        case State.String:
                            if (isEscaped)
                            {
                                switch (c)
                                {
                                    case 'n':
                                        sb.Append('\n');
                                        break;
                                    case 'r':
                                        sb.Append('\r');
                                        break;
                                    case 't':
                                        sb.Append('\t');
                                        break;
                                    case 'v':
                                        sb.Append('\v');
                                        break;
                                    case '\\':
                                        sb.Append('\\');
                                        break;
                                    case '\'':
                                        sb.Append('\'');
                                        break;
                                    case '"':
                                        sb.Append('"');
                                        break;
                                        // TODO: unicode escapes
                                }
                                isEscaped = false;
                            }
                            else if (c == '\\')
                            {
                                isEscaped = true;
                            }
                            else if (c == '"')
                            {
                                value = sb.ToString();
                                sb.Clear();
                                state = State.End1;
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                        case State.End1:
                            if (char.IsWhiteSpace(c))
                            {
                                continue;
                            }
                            else if (c == '/')
                            {
                                c = reader.ReadChar();
                                if (c == '/')
                                {
                                    state = State.CommentPreEnd;
                                }
                            }
                            else if (c == ';')
                            {
                                state = State.CommentPreEnd;
                            }
                            // TODO: multiline comments
                            else if (c == 'E' || c == 'e')
                            {
                                state = State.End2;
                            }
                            else
                            {
                                // throw new InvalidDataException($"Unexpected token {c}.");
                                // There is a typo in BFME2's str which seems to be just ignored (string ends with two '"')
                            }
                            break;
                        case State.End2:
                            if (c == 'N' || c == 'n')
                            {
                                state = State.End3;
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected token {c}.");
                            }
                            break;
                        case State.End3:
                            if (c == 'D' || c == 'd')
                            {
                                if (!str._strings.TryGetValue(category, out var dict))
                                {
                                    dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                    str._strings.Add(category, dict);
                                }
                                if (dict.ContainsKey(label))
                                {
                                    logger.Info($"[STR] String duplication: '{category}:{label}' -> '{dict[label]}', new value: '{value}'");
                                }
                                else
                                {
                                    dict.Add(label, value);
                                    ++str._numStrings;
                                }
                                state = State.Begin;
                            }
                            else
                            {
                                throw new InvalidDataException($"Unexpected token {c}.");
                            }
                            break;
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

        private Str _str;

        public override string Name => NameOverride ?? _str._language;
        public override IReadOnlyCollection<string> Labels
        {
            get
            {
                var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var label in _str._strings)
                {
                    foreach (var str in label.Value)
                    {
                        result.Add($"{label.Key}:{str.Key}");
                    }
                }
                return result;
            }
        }

        public StrTranslationProvider(Stream stream, string language)
        {
            Debug.Assert(!(stream is null), $"{nameof(stream)} is null");
            _str = new Str();
            Str.ReadStr(_str, stream, language);
        }


        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override string GetString(string str)
        {
            Debug.Assert(!(str is null), $"{nameof(str)} is null");
            if (!_str.TryGetString(str, out var result))
            {
                logger.Warn($"Requested string '{str}' not found in '{Name}'.");
            }
            return result;
        }

        public override string ToString()
        {
            return $"[STR: {Name} - {_str._numStrings} strings in {_str._strings.Count} categories]";
        }
    }
}
