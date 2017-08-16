using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CreateObjectDieBehavior : ObjectBehavior
    {
        internal static CreateObjectDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateObjectDieBehavior> FieldParseTable = new IniParseTable<CreateObjectDieBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "CreationList", (parser, x) => x.CreationList = parser.ParseAssetReference() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public string CreationList { get; private set; }
    }
}
