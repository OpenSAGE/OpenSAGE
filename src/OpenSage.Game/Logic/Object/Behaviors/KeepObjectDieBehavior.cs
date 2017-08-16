using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class KeepObjectDieBehavior : ObjectBehavior
    {
        internal static KeepObjectDieBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<KeepObjectDieBehavior> FieldParseTable = new IniParseTable<KeepObjectDieBehavior>();
    }
}
