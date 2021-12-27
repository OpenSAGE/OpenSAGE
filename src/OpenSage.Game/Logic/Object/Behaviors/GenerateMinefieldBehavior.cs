using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class GenerateMinefieldBehavior : BehaviorModule, IUpgradeableModule
    {
        private readonly UpgradeLogic _upgradeLogic;
        private bool _unknown;

        internal GenerateMinefieldBehavior(GenerateMinefieldBehaviorModuleData moduleData)
        {
            _upgradeLogic = new UpgradeLogic(moduleData.UpgradeData, this);
        }

        void IUpgradeableModule.OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            // TODO
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _upgradeLogic.Load(reader);

            _unknown = reader.ReadBoolean();

            reader.SkipUnknownBytes(13);
        }
    }

    public sealed class GenerateMinefieldBehaviorModuleData : UpdateModuleData
    {
        internal static GenerateMinefieldBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GenerateMinefieldBehaviorModuleData> FieldParseTable =
            new IniParseTableChild<GenerateMinefieldBehaviorModuleData, UpgradeLogicData>(x => x.UpgradeData, UpgradeLogicData.FieldParseTable)
            .Concat(new IniParseTable<GenerateMinefieldBehaviorModuleData>
            {
                { "MineName", (parser, x) => x.MineName = parser.ParseAssetReference() },
                { "DistanceAroundObject", (parser, x) => x.DistanceAroundObject = parser.ParseInteger() },
                { "GenerateOnlyOnDeath", (parser, x) => x.GenerateOnlyOnDeath = parser.ParseBoolean() },
                { "SmartBorder", (parser, x) => x.SmartBorder = parser.ParseBoolean() },
                { "SmartBorderSkipInterior", (parser, x) => x.SmartBorderSkipInterior = parser.ParseBoolean() },
                { "AlwaysCircular", (parser, x) => x.AlwaysCircular = parser.ParseBoolean() },
                { "GenerationFX", (parser, x) => x.GenerationFX = parser.ParseAssetReference() },
                { "Upgradable", (parser, x) => x.Upgradable = parser.ParseBoolean() },
                { "UpgradedTriggeredBy", (parser, x) => x.UpgradedTriggeredBy = parser.ParseAssetReference() },
                { "UpgradedMineName", (parser, x) => x.UpgradedMineName = parser.ParseAssetReference() },
            });

        public UpgradeLogicData UpgradeData { get; } = new();

        public string MineName { get; private set; }
        public int DistanceAroundObject { get; private set; }
        public bool GenerateOnlyOnDeath { get; private set; }
        public bool SmartBorder { get; private set; }
        public bool SmartBorderSkipInterior { get; private set; }
        public bool AlwaysCircular { get; private set; }
        public string GenerationFX { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool Upgradable { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string UpgradedTriggeredBy { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string UpgradedMineName { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GenerateMinefieldBehavior(this);
        }
    }
}
