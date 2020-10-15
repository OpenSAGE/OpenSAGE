using Veldrid;

namespace OpenSage.Input
{
    static class KeyMap
    {
        internal static char GetCharacter(Key key)
        {
            switch (key)
            {
                case Key k when k >= Key.A && k <= Key.Z:
                    return k.ToString().ToLower()[0];
                case Key.Period:
                    return '.';
                case Key.Space:
                    return ' ';
                case Key.Minus:
                    return '-';
                case Key k when k >= Key.Number0 && k <= Key.Number9:
                    return (char)('0' + (k - Key.Number0));
                default:
                    return '\0';
            }      
        }
    }
}
