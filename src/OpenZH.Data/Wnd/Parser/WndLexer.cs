using System.IO;
using System.Text;

namespace OpenZH.Data.Wnd.Parser
{
    internal sealed class WndLexer
    {
        private readonly string _source;
        private int _currentIndex;

        public WndLexer(string source)
        {
            _source = source;
            _currentIndex = 0;
        }

        public WndToken Lex()
        {
            while (char.IsWhiteSpace(CurrentChar))
            {
                NextChar();
            }

            var c = CurrentChar;
            NextChar();

            switch (c)
            {
                case '\0':
                    return new WndToken(WndTokenType.EndOfFile);

                case '=':
                    return new WndToken(WndTokenType.Equals);

                case ';':
                    return new WndToken(WndTokenType.Semicolon);

                case ':':
                    return new WndToken(WndTokenType.Colon);

                case '[':
                    return new WndToken(WndTokenType.OpenSquareBracket);

                case ']':
                    return new WndToken(WndTokenType.CloseSquareBracket);

                case ',':
                    return new WndToken(WndTokenType.Comma);

                case '+':
                    return new WndToken(WndTokenType.Plus);

                case '"':
                    var sb = new StringBuilder();
                    while (CurrentChar != '"')
                    {
                        sb.Append(CurrentChar);
                        NextChar();
                    }
                    NextChar();
                    return new WndToken(WndTokenType.StringLiteral)
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
                    var integerValue = c.ToString();
                    while (char.IsDigit(CurrentChar))
                    {
                        integerValue += CurrentChar;
                        NextChar();
                    }
                    return new WndToken(WndTokenType.IntegerLiteral)
                    {
                        IntegerValue = int.Parse(integerValue)
                    };

                default:
                    if (char.IsLetter(c))
                    {
                        var identiferValue = new StringBuilder();
                        identiferValue.Append(c);

                        while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_' || CurrentChar == '-')
                        {
                            identiferValue.Append(CurrentChar);
                            NextChar();
                        }

                        return new WndToken(WndTokenType.Identifier)
                        {
                            StringValue = identiferValue.ToString()
                        };
                    }

                    throw new InvalidDataException($"Unexpected character: {c}");
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

        private void NextChar()
        {
            _currentIndex++;
        }
    }
}
