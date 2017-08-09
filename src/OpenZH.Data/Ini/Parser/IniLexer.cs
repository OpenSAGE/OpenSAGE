using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenZH.Data.Ini.Parser
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

        public IniToken Lex()
        {
            SkipWhitespaceAndComments();

            if (_anyNonWhitespaceOnLine && CurrentChar == '\n')
            {
                _anyNonWhitespaceOnLine = false;

                NextChar();

                return CreateToken(IniTokenType.EndOfLine);
            }

            _anyNonWhitespaceOnLine = true;

            var c = CurrentChar;
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
                    var sb = new StringBuilder();
                    while (CurrentChar != '"')
                    {
                        sb.Append(CurrentChar);
                        NextChar();
                    }
                    NextChar();
                    return new IniToken(IniTokenType.StringLiteral, CurrentPosition)
                    {
                        StringValue = sb.ToString()
                    };

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
                case '.':
                    return LexNumber(c);

                default:
                    if (char.IsLetter(c))
                    {
                        var identiferValue = new StringBuilder();
                        identiferValue.Append(c);

                        while (IsIdentifierChar(CurrentChar))
                        {
                            identiferValue.Append(CurrentChar);
                            NextChar();
                        }

                        return new IniToken(IniTokenType.Identifier, CurrentPosition)
                        {
                            StringValue = identiferValue.ToString()
                        };
                    }

                    throw new IniParseException($"Unexpected character: {c}", CurrentPosition);
            }
        }

        private static bool IsIdentifierChar(char c)
        {
            if (char.IsLetterOrDigit(c))
            {
                return true;
            }

            switch (c)
            {
                case '_':
                case '-':
                case ':':
                case '\\':
                case '.':
                    return true;

                default:
                    return false;
            }
        }

        private IniToken LexNumber(char c)
        {
            var numDots = c == '.' ? 1 : 0;

            var numberValue = c.ToString();
            while (char.IsDigit(CurrentChar) || CurrentChar == '.')
            {
                if (CurrentChar == '.')
                {
                    numDots++;
                }
                numberValue += CurrentChar;
                NextChar();
            }

            if (numDots > 1)
            {
                throw new IniParseException($"Invalid number: {numberValue}", CurrentPosition);
            }

            var tokenType = numDots > 0
                ? IniTokenType.FloatLiteral
                : IniTokenType.IntegerLiteral;

            if (CurrentChar == '%')
            {
                tokenType = IniTokenType.PercentLiteral;
                NextChar();
            }

            var result = new IniToken(tokenType, CurrentPosition);

            switch (tokenType)
            {
                case IniTokenType.FloatLiteral:
                case IniTokenType.PercentLiteral:
                    return new IniToken(tokenType, CurrentPosition)
                    {
                        FloatValue = float.Parse(numberValue)
                    };

                case IniTokenType.IntegerLiteral:
                    return new IniToken(tokenType, CurrentPosition)
                    {
                        IntegerValue = int.Parse(numberValue)
                    };

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IniTokenPosition CurrentPosition => new IniTokenPosition(_fileName, _currentLine, _currentCharacter);

        private IniToken CreateToken(IniTokenType tokenType) => new IniToken(tokenType, CurrentPosition);

        private void SkipWhitespaceAndComments()
        {
            while (char.IsWhiteSpace(CurrentChar) || CurrentChar == ';')
            {
                if (CurrentChar == ';')
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
}
