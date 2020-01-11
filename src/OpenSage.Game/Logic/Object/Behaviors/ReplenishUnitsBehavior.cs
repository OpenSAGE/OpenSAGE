using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ReplenishUnitsBehaviorModuleData : BehaviorModuleData
    {
        internal static ReplenishUnitsBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<ReplenishUnitsBehaviorModuleData> BaseFieldParseTable = new IniParseTable<ReplenishUnitsBehaviorModuleData>
        {
            { "ReplenishDelay", (parser, x) => x.ReplenishDelay = parser.ParseInteger() },
            { "ReplenishRadius", (parser, x) => x.ReplenishRadius = parser.ParseFloat() },
            { "NoReplenishIfEnemyWithinRadius", (parser, x) => x.NoReplenishIfEnemyWithinRadius = parser.ParseFloat() },
            { "ReplenishStatii", (parser, x) => x.ReplenishStatii = parser.ParseEnum<ObjectStatus>() },
            { "ReplenishFXList", (parser, x) => x.ReplenishFXList = parser.ParseAssetReference() },
            { "ReplenishHordeMembersOnly", (parser, x) => x.ReplenishHordeMembersOnly = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
        };

        public int ReplenishDelay { get; private set; }
        public float ReplenishRadius { get; private set; }
        public float NoReplenishIfEnemyWithinRadius { get; private set; }
        public ObjectStatus ReplenishStatii { get; private set; }
        public string ReplenishFXList { get; private set; }
        public bool ReplenishHordeMembersOnly { get; private set; }
        public bool StartsActive { get; private set; }
    }
}
