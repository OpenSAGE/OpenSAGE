using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    internal sealed class ReallyLowMHz
    {
        public static int Parse(IniParser parser)
        {
            return parser.ParseInteger();
        }
    }
}
