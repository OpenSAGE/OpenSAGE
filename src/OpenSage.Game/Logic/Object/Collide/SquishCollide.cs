using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SquishCollide : ObjectBehavior
    {
        internal static SquishCollide Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SquishCollide> FieldParseTable = new IniParseTable<SquishCollide>();
    }
}
