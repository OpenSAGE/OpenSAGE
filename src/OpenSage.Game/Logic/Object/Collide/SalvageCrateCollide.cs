using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class SalvageCrateCollide : CrateCollide
    {
        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

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
        public Percentage WeaponChance { get; private set; }
        public Percentage LevelChance { get; private set; }
        public Percentage MoneyChance { get; private set; }
        public int MinMoney { get; private set; }
        public int MaxMoney { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ExecuteFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage PorterChance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage BannerChance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage LevelUpChance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float LevelUpRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage ResourceChance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MinResource { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MaxResource { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AllowAIPickup { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SalvageCrateCollide();
        }
    }
}
