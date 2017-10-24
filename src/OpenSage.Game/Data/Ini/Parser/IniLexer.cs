using System;
using System.Text;
using static OpenSage.Data.Utilities.ParseUtility;

namespace OpenSage.Data.Ini.Parser
{
    internal sealed class IniLexer
    {
        private readonly string _source;
        private readonly string _fileName;

        private int _currentIndex;
        private bool _anyNonWhitespaceOnLine;

        private int _currentLine;
        private int _currentCharacter;

        public IniLexer(string source, string fileName)
        {
            _source = source.Replace("\r\n", "\n"); // Normalise line endings
            _fileName = fileName;

            _currentIndex = 0;

            _currentLine = 1;
            _currentCharacter = 1;
        }

        public IniLexerState GetState()
        {
            return new IniLexerState
            {
                CurrentIndex = _currentIndex,
                AnyNonWhitespaceOnLine = _anyNonWhitespaceOnLine,

                CurrentLine = _currentLine,
                CurrentCharacter = _currentCharacter
            };
        }

        public void RestoreState(IniLexerState state)
        {
            _currentIndex = state.CurrentIndex;
            _anyNonWhitespaceOnLine = state.AnyNonWhitespaceOnLine;

            _currentLine = state.CurrentLine;
            _currentCharacter = state.CurrentCharacter;
        }

        public IniToken Lex(IniLexerMode mode)
        {
            SkipWhitespaceAndComments();

            if (_anyNonWhitespaceOnLine && CurrentChar == '\n')
            {
                _anyNonWhitespaceOnLine = false;

                NextChar();

                return CreateToken(IniTokenType.EndOfLine);
            }

            _anyNonWhitespaceOnLine = true;

            var pos = CurrentPosition;
            var c = CurrentChar;

            if (mode != IniLexerMode.Normal)
            {
                var sb = new StringBuilder();
                if (mode == IniLexerMode.String && CurrentChar == '"')
                {
                    NextChar();
                    return LexQuotedString(pos);
                }
                bool isStringChar()
                {
                    if (mode != IniLexerMode.StringWithWhitespace && char.IsWhiteSpace(CurrentChar))
                        return false;
                    return CurrentChar != '\0' && CurrentChar != ';' && CurrentChar != '\n';
                }

                while (isStringChar())
                {
                    sb.Append(CurrentChar);
                    NextChar();
                }
                if (sb.Length > 0)
                {
                    return new IniToken(IniTokenType.StringLiteral, pos)
                    {
                        StringValue = sb.ToString()
                    };
                }
            }

            NextChar();

            switch (c)
            {
                case '\0':
                    return CreateToken(IniTokenType.EndOfFile);

                case '=':
                    return CreateToken(IniTokenType.Equals);

                case ':':
                    return CreateToken(IniTokenType.Colon);

                case ',':
                    return CreateToken(IniTokenType.Comma);

                case '"':
                    return LexQuotedString(pos);

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                case '+':
                case '.':
                    return LexNumber(c, pos);

                case '#':
                    return LexDefineKeyword(c, pos);

                default:
                    if (IsIdentifierStartChar(c))
                    {
                        var identiferValue = new StringBuilder();
                        identiferValue.Append(c);

                        while (IsIdentifierChar(CurrentChar))
                        {
                            identiferValue.Append(CurrentChar);
                            NextChar();
                        }

                        return new IniToken(IniTokenType.Identifier, pos)
                        {
                            StringValue = identiferValue.ToString()
                        };
                    }

                    throw new IniParseException($"Unexpected character: {c}", pos);
            }
        }

        private static bool IsIdentifierStartChar(char c) => char.IsLetter(c) || c == '_';

        private static bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '%';

        private IniToken LexQuotedString(IniTokenPosition pos)
        {
            var sb = new StringBuilder();
            while (CurrentChar != '"')
            {
                sb.Append(CurrentChar);
                NextChar();
            }
            NextChar();
            return new IniToken(IniTokenType.StringLiteral, pos)
            {
                StringValue = sb.ToString()
            };
        }

        private IniToken LexDefineKeyword(char c, IniTokenPosition pos)
        {
            var sb = new StringBuilder(c.ToString());
            while (!char.IsWhiteSpace(CurrentChar))
            {
                sb.Append(CurrentChar);
                NextChar();
            }
            return new IniToken(IniTokenType.DefineKeyword, pos)
            {
                StringValue = sb.ToString()
            };
        }

        private IniToken LexNumber(char c, IniTokenPosition pos)
        {
            var hasDot = c == '.';

            var numberValue = c.ToString();
            while (char.IsDigit(CurrentChar) || CurrentChar == '.' || CurrentChar == 'v')
            {
                if (CurrentChar == '.')
                {
                    if (hasDot)
                    {
                        // ODDITY: In CivilianProp.ini:6893, there are two dots in the number: 18.5.0
                        NextChar();
                        continue;
                    }
                    hasDot = true;
                }
                else if (CurrentChar == 'v')
                {
                    // ODDITY: In CivilianBuilding.ini:20199, there's a "v" in the middle of a number.
                    if (PeekChar() == '.')
                    {
                        NextChar();
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                numberValue += CurrentChar;
                NextChar();
            }

            var tokenType = hasDot
                ? IniTokenType.FloatLiteral
                : IniTokenType.IntegerLiteral;

            if (CurrentChar == '%')
            {
                tokenType = IniTokenType.PercentLiteral;
                NextChar();
            }
            else if (IsIdentifierChar(CurrentChar))
            {
                var numIdentifierChars = 0;
                while (IsIdentifierChar(CurrentChar))
                {
                    numberValue += CurrentChar;
                    NextChar();
                    numIdentifierChars++;
                }
                if (numIdentifierChars == 1 && numberValue.EndsWith("f"))
                {
                    return new IniToken(tokenType, CurrentPosition)
                    {
                        FloatValue = ParseFloat(numberValue.TrimEnd('f'))
                    };
                }
                if (hasDot)
                {
                    throw new IniParseException($"Invalid number: {numberValue}", CurrentPosition);
                }
                return new IniToken(IniTokenType.Identifier, CurrentPosition)
                {
                    StringValue = numberValue
                };
            }

            switch (tokenType)
            {
                case IniTokenType.FloatLiteral:
                case IniTokenType.PercentLiteral:
                    return new IniToken(tokenType, pos)
                    {
                        FloatValue = ParseFloat(numberValue)
                    };

                case IniTokenType.IntegerLiteral:
                    var longNumber = long.Parse(numberValue);
                    if (Math.Abs(longNumber) <= int.MaxValue)
                    {
                        return new IniToken(tokenType, pos)
                        {
                            IntegerValue = (int) longNumber
                        };
                    }
                    return new IniToken(tokenType, pos)
                    {
                        LongValue = longNumber
                    };

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IniTokenPosition CurrentPosition => new IniTokenPosition(_fileName, _currentLine, _currentCharacter);

        private IniToken CreateToken(IniTokenType tokenType) => new IniToken(tokenType, CurrentPosition);

        private void SkipWhitespaceAndComments()
        {
            while (char.IsWhiteSpace(CurrentChar) || CurrentChar == ';' || (CurrentChar == '/' && PeekChar() == '/'))
            {
                if (CurrentChar == ';' || CurrentChar == '/')
                {
                    while (CurrentChar != '\n' && CurrentChar != '\0')
                    {
                        NextChar();
                    }
                }
                else if (CurrentChar == '\n')
                {
                    if (_anyNonWhitespaceOnLine)
                    {
                        // Line break after non-whitespace-only line is significant.
                        return;
                    }
                    else
                    {
                        NextChar();
                    }
                }
                else
                {
                    NextChar();
                }
            }
        }

        private char CurrentChar
        {
            get
            {
                return (_currentIndex < _source.Length)
                    ? _source[_currentIndex]
                    : '\0';
            }
        }

        private char PeekChar()
        {
            return (_currentIndex + 1 < _source.Length)
                ? _source[_currentIndex + 1]
                : '\0';
        }

        private void NextChar()
        {
            if (CurrentChar == '\n')
            {
                _currentLine++;
                _currentCharacter = 0;
            }
            else
            {
                _currentCharacter++;
            }

            _currentIndex++;
        }
    }

    internal struct IniLexerState
    {
        public int CurrentIndex;
        public bool AnyNonWhitespaceOnLine;

        public int CurrentLine;
        public int CurrentCharacter;
    }

    internal enum IniLexerMode
    {
        Normal,
        String,
        StringWithWhitespace,
        AssetReference
    }
}
