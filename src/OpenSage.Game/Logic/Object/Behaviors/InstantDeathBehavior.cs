using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class InstantDeathBehavior : ObjectBehavior
    {
        internal static InstantDeathBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InstantDeathBehavior> FieldParseTable = new IniParseTable<InstantDeathBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "FX", (parser, x) => x.FX = parser.ParseAssetReference() },
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public string FX { get; private set; }
    }
}
