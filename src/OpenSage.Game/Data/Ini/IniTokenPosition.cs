namespace OpenSage.Data.Ini
{
    internal readonly struct IniTokenPosition
    {
        public static readonly IniTokenPosition None = new IniTokenPosition(null, 0, 0);

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
