using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SalvageCrateCollideModuleData : CrateCollideModuleData
    {
        internal static SalvageCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SalvageCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<SalvageCrateCollideModuleData>
            {
                { "PickupScience", (parser, x) => x.PickupScience = parser.ParseAssetReference() },
                { "WeaponChance", (parser, x) => x.WeaponChance = parser.ParsePercentage() },
                { "LevelChance", (parser, x) => x.LevelChance = parser.ParsePercentage() },
                { "MoneyChance", (parser, x) => x.MoneyChance = parser.ParsePercentage() },
                { "MinMoney", (parser, x) => x.MinMoney = parser.ParseInteger() },
                { "MaxMoney", (parser, x) => x.MaxMoney = parser.ParseInteger() },

                { "ExecuteFX", (parser, x) => x.PickupScience = parser.ParseAssetReference() },
                { "PorterChance", (parser, x) => x.PorterChance = parser.ParsePercentage() },
                { "BannerChance", (parser, x) => x.BannerChance = parser.ParsePercentage() },
                { "LevelUpChance", (parser, x) => x.LevelUpChance = parser.ParsePercentage() },
                { "LevelUpRadius", (parser, x) => x.LevelUpRadius = parser.ParseFloat() },
                { "ResourceChance", (parser, x) => x.ResourceChance = parser.ParsePercentage() },
                { "MinResource", (parser, x) => x.MinResource = parser.ParseInteger() },
                { "MaxResource", (parser, x) => x.MaxResource = parser.ParseInteger() },
                { "AllowAIPickup", (parser, x) => x.AllowAIPickup = parser.ParseBoolean() },
            });

        public string PickupScience { get; private set; }
        public float WeaponChance { get; private set; }
        public float LevelChance { get; private set; }
        public float MoneyChance { get; private set; }
        public int MinMoney { get; private set; }
        public int MaxMoney { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string ExecuteFX { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float PorterChance { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float BannerChance { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float LevelUpChance { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float LevelUpRadius { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float ResourceChance { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int MinResource { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int MaxResource { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool AllowAIPickup { get; private set; }
    }
}
