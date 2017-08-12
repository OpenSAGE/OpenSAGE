using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    internal sealed class ReallyLowMHz
    {
        public static int Parse(IniParser parser)
        {
            parser.NextToken(IniTokenType.Identifier);
            parser.NextToken(IniTokenType.Equals);

            var result = parser.ParseInteger();

            parser.NextToken(IniTokenType.EndOfLine);

            return result;
        }
    }
}
