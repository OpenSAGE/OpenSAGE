using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class MoneyCrateCollide : ObjectBehavior
    {
        internal static MoneyCrateCollide Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MoneyCrateCollide> FieldParseTable = new IniParseTable<MoneyCrateCollide>
        {
            { "BuildingPickup", (parser, x) => x.BuildingPickup = parser.ParseBoolean() },
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "MoneyProvided", (parser, x) => x.MoneyProvided = parser.ParseInteger() },
            { "ExecuteAnimation", (parser, x) => x.ExecuteAnimation = parser.ParseAssetReference() },
            { "ExecuteAnimationTime", (parser, x) => x.ExecuteAnimationTime = parser.ParseFloat() },
            { "ExecuteAnimationZRise", (parser, x) => x.ExecuteAnimationZRise = parser.ParseFloat() },
            { "ExecuteAnimationFades", (parser, x) => x.ExecuteAnimationFades = parser.ParseBoolean() },
            { "ForbidOwnerPlayer", (parser, x) => x.ForbidOwnerPlayer = parser.ParseBoolean() },
            { "HumanOnly", (parser, x) => x.HumanOnly = parser.ParseBoolean() },


            { "UpgradedBoost", (parser, x) => x.UpgradedBoost = BoostUpgrade.Parse(parser) }
        };

        public bool BuildingPickup { get; private set; }
        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int MoneyProvided { get; private set; }
        public string ExecuteAnimation { get; private set; }
        public float ExecuteAnimationTime { get; private set; }
        public float ExecuteAnimationZRise { get; private set; }
        public bool ExecuteAnimationFades { get; private set; }
        public bool ForbidOwnerPlayer { get; private set; }
        public bool HumanOnly { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public BoostUpgrade UpgradedBoost { get; private set; }
    }

    public struct BoostUpgrade
    {
        internal static BoostUpgrade Parse(IniParser parser)
        {
            return new BoostUpgrade
            {
                UpgradeType = parser.ParseAttribute("UpgradeType", () => parser.ParseAssetReference()),
                Boost = parser.ParseAttributeInteger("Boost")
            };
        }

        public string UpgradeType;
        public int Boost;
    }
}
