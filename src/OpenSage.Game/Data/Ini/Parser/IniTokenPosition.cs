namespace OpenSage.Data.Ini.Parser
{

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
