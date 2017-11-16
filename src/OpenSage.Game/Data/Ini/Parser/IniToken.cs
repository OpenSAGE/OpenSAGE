namespace OpenSage.Data.Ini.Parser
{
    internal struct IniToken
    {
        public string Text;
        public IniTokenPosition Position;

        public IniToken(string text, IniTokenPosition position)
        {
            Text = text;
            Position = position;
        }
    }
}
