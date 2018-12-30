namespace OpenSage.Data.Ini.Parser
{
    public readonly struct IniTokenPosition
    {
        public readonly string File;
        public readonly int Line;
        public readonly int Character;

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
