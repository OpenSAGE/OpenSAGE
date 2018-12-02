namespace OpenSage.Data.Ini.Parser
{
    public struct IniToken
    {
        public readonly string Text;
        public readonly IniTokenPosition Position;

        public IniToken(string text, IniTokenPosition position)
        {
            Text = text;
            Position = position;
        }
    }
}
