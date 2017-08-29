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
            });

        public string PickupScience { get; private set; }
        public float WeaponChance { get; private set; }
        public float LevelChance { get; private set; }
        public float MoneyChance { get; private set; }
        public int MinMoney { get; private set; }
        public int MaxMoney { get; private set; }
    }
}
