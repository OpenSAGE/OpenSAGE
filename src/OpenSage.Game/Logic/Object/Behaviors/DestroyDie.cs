using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DestroyDie : ObjectBehavior
    {
        internal static DestroyDie Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<DestroyDie> FieldParseTable = new IniParseTable<DestroyDie>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
    }
}
