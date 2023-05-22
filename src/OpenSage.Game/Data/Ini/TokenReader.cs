using System;
using System.Linq;

namespace OpenSage.Data.Ini
{
    internal sealed class TokenReader
    {
        private readonly string _source;
        private readonly string _fileName;

        private int _sourceTextIndex;
        private ReadOnlyMemory<char> _currentLineText;
        private int _currentLine;
        private int _currentLineCharIndex;

        public bool EndOfFile { get; private set; }

        private IniToken? _peekedToken;
        private char[] _peekedTokenSeparators;
        private int _nextCharIndex;

        public IniTokenPosition CurrentPosition => new IniTokenPosition(
            _fileName,
            _currentLine + 1,
            _currentLineCharIndex + 1);

        public TokenReader(string source, string fileName)
        {
            _source = source;
            _fileName = fileName;

            _currentLine = -1;
        }

        private void SetCurrentLine(ReadOnlyMemory<char> text)
        {
            _currentLineText = text;
            _currentLineCharIndex = 0;
            _peekedToken = null;
        }

        public void GoToNextLine()
        {
            if (EndOfFile)
            {
                SetCurrentLine(string.Empty.AsMemory());
                return;
            }

            var startIndex = _sourceTextIndex;
            var length = 0;

            var afterComment = false;
            while (true)
            {
                if (_sourceTextIndex >= _source.Length)
                {
                    EndOfFile = true;
                    break;
                }

                var c = _source[_sourceTextIndex++];

                // Reached end of line.
                if (c == '\n')
                {
                    break;
                }

                // Handle comments.
                if (c == ';' || (c == '/' && _sourceTextIndex < _source.Length && _source[_sourceTextIndex] == '/')
                    || (c == '-' && _sourceTextIndex < _source.Length && _source[_sourceTextIndex] == '-'))
                {
                    afterComment = true;
                }
                else if (!afterComment)
                {
                    length++;
                }
            }

            SetCurrentLine(_source.AsMemory(startIndex, length));

            _currentLine++;
        }

        private (IniToken?, int nextCharIndex) ReadToken(char[] separators)
        {
            if (_currentLineCharIndex >= _currentLineText.Length)
            {
                return (null, _currentLineCharIndex);
            }

            var nextCharIndex = _currentLineCharIndex;

            // Skip leading trivia.
            while (nextCharIndex < _currentLineText.Length
                   && Array.IndexOf(separators, _currentLineText.Span[nextCharIndex]) != -1)
            {
                nextCharIndex++;
            }

            var startIndex = nextCharIndex;
            var length = 0;

            var position = new IniTokenPosition(_fileName, _currentLine + 1, nextCharIndex + 1);

            while (nextCharIndex < _currentLineText.Length
                   && Array.IndexOf(separators, _currentLineText.Span[nextCharIndex]) == -1)
            {
                length++;
                nextCharIndex++;
            }

            // Skip trailing separator.
            if (nextCharIndex < _currentLineText.Length
                && Array.IndexOf(separators, _currentLineText.Span[nextCharIndex]) != -1)
            {
                nextCharIndex++;
            }

            var result = _currentLineText.Slice(startIndex, length);

            if (result.Length == 0)
            {
                return (null, nextCharIndex);
            }

            return (new IniToken(result, position), nextCharIndex);
        }

        public IniToken? PeekToken(char[] separators)
        {
            if (TryGetPeekedToken(separators, out var peekedToken))
            {
                return peekedToken;
            }

            var (token, nextIndex) = ReadToken(separators);
            _peekedToken = token;
            _peekedTokenSeparators = separators;
            _nextCharIndex = nextIndex;
            return token;
        }

        public IniToken? NextToken(char[] separators)
        {
            if (TryGetPeekedToken(separators, out var peekedToken))
            {
                _currentLineCharIndex = _nextCharIndex;
                _peekedToken = null;
                return peekedToken;
            }

            var (token, nextIndex) = ReadToken(separators);
            _currentLineCharIndex = nextIndex;
            return token;
        }

        private bool TryGetPeekedToken(char[] separators, out IniToken? token)
        {
            // Note that this is a reference equality check.
            // We assume we'll use the same separator arrays for the entire parsing session.
            if (_peekedToken.HasValue && _peekedTokenSeparators == separators)
            {
                token = _peekedToken;
                return true;
            }

            token = null;
            return false;
        }
    }
}
