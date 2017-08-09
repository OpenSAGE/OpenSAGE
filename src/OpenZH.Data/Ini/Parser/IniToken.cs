namespace OpenZH.Data.Ini.Parser
{
    internal struct IniToken
    {
        public IniTokenType TokenType;
        public string StringValue;
        public int IntegerValue;
        public float FloatValue;

        public IniTokenPosition Position;

        public IniToken(IniTokenType tokenType, IniTokenPosition position)
        {
            TokenType = tokenType;
            StringValue = null;
            IntegerValue = int.MinValue;
            FloatValue = float.MinValue;

            Position = position;
        }

        public override string ToString()
        {
            return $"Type: {TokenType}; StringValue: {StringValue}; IntegerValue: {IntegerValue}";
        }
    }

    internal struct IniTokenPosition
    {
        public string File;
        public int Line;
        public int Character;

        public IniTokenPosition(string file, int line, int character)
        {
            File = file;
            Line = line;
            Character = character;
        }

        public override string ToString()
        {
            return $"{File},{Line},{Character}";
        }
    }
}
