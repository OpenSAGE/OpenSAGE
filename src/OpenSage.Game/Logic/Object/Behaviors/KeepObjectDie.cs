using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class KeepObjectDie : ObjectBehavior
    {
        internal static KeepObjectDie Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<KeepObjectDie> FieldParseTable = new IniParseTable<KeepObjectDie>();
    }
}
