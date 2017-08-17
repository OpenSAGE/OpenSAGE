using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class UnitCrateCollideBehavior : ObjectBehavior
    {
        internal static UnitCrateCollideBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<UnitCrateCollideBehavior> FieldParseTable = new IniParseTable<UnitCrateCollideBehavior>
        {
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "UnitCount", (parser, x) => x.UnitCount = parser.ParseInteger() },
            { "UnitName", (parser, x) => x.UnitName = parser.ParseAssetReference() }
        };

        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int UnitCount { get; private set; }
        public string UnitName { get; private set; }
    }
}
