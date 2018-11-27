using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class CastleMemberBehaviorModuleData : BehaviorModuleData
    {
        internal static CastleMemberBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<CastleMemberBehaviorModuleData> FieldParseTable = new IniParseTable<CastleMemberBehaviorModuleData>
        {
            { "CountsForEvaCastleBreached", (parser, x) => x.CountsForEvaCastleBreached = parser.ParseBoolean() },
            { "UnderAttackEvaEventIfKeep", (parser, x) => x.UnderAttackEvaEventIfKeep = parser.ParseAssetReference() },
            { "UnderAttackAllyEvaEventIfKeep", (parser, x) => x.UnderAttackAllyEvaEventIfKeep = parser.ParseAssetReference() },
            { "CampDestroyedOwnerEvaEvent", (parser, x) => x.CampDestroyedOwnerEvaEvent = parser.ParseAssetReference() },
            { "CampDestroyedAllyEvaEvent", (parser, x) => x.CampDestroyedAllyEvaEvent = parser.ParseAssetReference() },
            { "CampDestroyedAttackerEvaEvent", (parser, x) => x.CampDestroyedAttackerEvaEvent = parser.ParseAssetReference() },
            { "StoreUpgradePrice", (parser, x) => x.StoreUpgradePrice = parser.ParseBoolean() },
            { "BeingBuiltSound", (parser, x) => x.BeingBuiltSound = parser.ParseAssetReference() }
        };

        public bool CountsForEvaCastleBreached { get; private set; }
        public string UnderAttackEvaEventIfKeep { get; private set; }
        public string UnderAttackAllyEvaEventIfKeep { get; private set; }
        public string CampDestroyedOwnerEvaEvent { get; private set; }
        public string CampDestroyedAllyEvaEvent { get; private set; }
        public string CampDestroyedAttackerEvaEvent { get; private set; }
        public bool StoreUpgradePrice { get; private set; }
        public string BeingBuiltSound { get; private set; }
    }
}
