using System.Linq;
using System.Text;

namespace OpenSage.Data.Ini.Parser
{
    internal sealed class TokenReader
    {
        private readonly string _source;
        private readonly string _fileName;

        private readonly StringBuilder _stringBuilder;

        private int _sourceTextIndex;
        private string _currentLineText;
        private int _currentLine;
        private int _currentLineCharIndex;
        
        public bool EndOfFile { get; private set; }

        public IniTokenPosition CurrentPosition => new IniTokenPosition(
            _fileName,
            _currentLine + 1,
            _currentLineCharIndex + 1);

        public TokenReader(string source, string fileName)
        {
            _source = source;
            _fileName = fileName;
            _stringBuilder = new StringBuilder(100);

            _currentLine = -1;
        }

        private void SetCurrentLine(string text)
        {
            _currentLineText = text;
            _currentLineCharIndex = 0;
        }

        public void GoToNextLine()
        {
            if (EndOfFile)
            {
                SetCurrentLine(string.Empty);
                return;
            }

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
                    _stringBuilder.Append(c);
                    break;
                }

                // Handle comments.
                if (c == ';' || (c == '/' && _sourceTextIndex < _source.Length && _source[_sourceTextIndex] == '/'))
                {
                    afterComment = true;
                }
                else if (!afterComment)
                {
                    _stringBuilder.Append(c);
                }
            }

            SetCurrentLine(_stringBuilder.ToString());

            _stringBuilder.Clear();

            _currentLine++;
        }

        public IniToken? NextToken(char[] separators)
        {
            if (_currentLineCharIndex >= _currentLineText.Length)
            {
                return null;
            }

            // Skip leading trivia.
            while (_currentLineCharIndex < _currentLineText.Length 
                && separators.Contains(_currentLineText[_currentLineCharIndex]))
            {
                _currentLineCharIndex++;
            }

            var position = CurrentPosition;

            while (_currentLineCharIndex < _currentLineText.Length 
                && !separators.Contains(_currentLineText[_currentLineCharIndex]))
            {
                _stringBuilder.Append(_currentLineText[_currentLineCharIndex]);
                _currentLineCharIndex++;
            }

            // Skip trailing separator.
            if (_currentLineCharIndex < _currentLineText.Length
                && separators.Contains(_currentLineText[_currentLineCharIndex]))
            {
                _currentLineCharIndex++;
            }

            var result = _stringBuilder.ToString();

            _stringBuilder.Clear();

            if (result.Length == 0)
            {
                return null;
            }

            return new IniToken(result, position);
        }
    }
}
