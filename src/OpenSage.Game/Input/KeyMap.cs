using Veldrid;

namespace OpenSage.Input
{
    static class KeyMap
    {
        internal static char GetCharacter(Key key)
        {
            switch (key)
            {
                case Key.A:
                case Key.B:
                case Key.C:
                case Key.D:
                case Key.E:
                case Key.F:
                case Key.G:
                case Key.H:
                case Key.I:
                case Key.J:
                case Key.K:
                case Key.L:
                case Key.M:
                case Key.N:
                case Key.O:
                case Key.P:
                case Key.Q:
                case Key.R:
                case Key.S:
                case Key.T:
                case Key.U:
                case Key.V:
                case Key.W:
                case Key.X:
                case Key.Y:
                case Key.Z:
                    return key.ToString().ToLower()[0];
                case Key.Space:
                    return ' ';
                case Key.Minus:
                    return '-';
                case Key.Number0:
                case Key.Number1:
                case Key.Number2:
                case Key.Number3:
                case Key.Number4:
                case Key.Number5:
                case Key.Number6:
                case Key.Number7:
                case Key.Number8:
                case Key.Number9:
                    return (char)(key - Key.Number0);
                default:
                    return '\0';
            }      
        }
    }
}
