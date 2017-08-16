using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SquishCollideBehavior : ObjectBehavior
    {
        internal static SquishCollideBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SquishCollideBehavior> FieldParseTable = new IniParseTable<SquishCollideBehavior>();
    }
}
