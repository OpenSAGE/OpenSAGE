using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DestroyDieBehavior : ObjectBehavior
    {
        internal static DestroyDieBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DestroyDieBehavior> FieldParseTable = new IniParseTable<DestroyDieBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
    }
}
