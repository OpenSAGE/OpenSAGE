namespace OpenSage.Data.Ini
{
    internal readonly struct IniToken
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
