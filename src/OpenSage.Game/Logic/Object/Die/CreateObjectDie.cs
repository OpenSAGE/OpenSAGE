using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CreateObjectDie : ObjectBehavior
    {
        internal static CreateObjectDie Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateObjectDie> FieldParseTable = new IniParseTable<CreateObjectDie>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
            { "CreationList", (parser, x) => x.CreationList = parser.ParseAssetReference() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public ObjectStatus ExemptStatus { get; private set; }
        public string CreationList { get; private set; }
    }
}
