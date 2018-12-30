namespace OpenSage.Data.Ini.Parser
{
    public readonly struct IniToken
    {
        public readonly string Text;
        public readonly IniTokenPosition Position;

        public IniToken(string text, in IniTokenPosition position)
        {
            Text = text;
            Position = position;
        }
    }
}
