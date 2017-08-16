using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FXListDieBehavior : ObjectBehavior
    {
        internal static FXListDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FXListDieBehavior> FieldParseTable = new IniParseTable<FXListDieBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "DeathFX", (parser, x) => x.DeathFX = parser.ParseAssetReference() },
            { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public string DeathFX { get; private set; }
        public bool OrientToObject { get; private set; }
    }
}
