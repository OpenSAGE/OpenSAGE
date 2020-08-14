using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class ChinookAIUpdate : SupplyAIUpdate
    {
        private ChinookAIUpdateModuleData _moduleData;

        internal ChinookAIUpdate(GameObject gameObject, ChinookAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        protected override int GetAdditionalValuePerSupplyBox(ScopedAssetCollection<UpgradeTemplate> upgrades)
        {
            var upgradeDefinition = upgrades.GetByName("Upgrade_AmericaSupplyLines");
            return GameObject.UpgradeAvailable(upgradeDefinition) ? _moduleData.UpgradedSupplyBoost : 0;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);
        }
    }

    /// <summary>
    /// Logic requires bones for either end of the rope to be defined as RopeEnd and RopeStart.
    /// Infantry (or tanks) can be made to rappel down a rope by adding CAN_RAPPEL to the object's 
    /// KindOf field. Having done that, the "RAPPELLING" ModelConditonState becomes available for 
    /// rappelling out of the object that has the rappel code of this module.
    /// </summary>
    public sealed class ChinookAIUpdateModuleData : SupplyAIUpdateModuleData
    {
        internal static new ChinookAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ChinookAIUpdateModuleData> FieldParseTable = SupplyAIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<ChinookAIUpdateModuleData>
            {
                { "NumRopes", (parser, x) => x.NumRopes = parser.ParseInteger() },
                { "PerRopeDelayMin", (parser, x) => x.PerRopeDelayMin = parser.ParseInteger() },
                { "PerRopeDelayMax", (parser, x) => x.PerRopeDelayMax = parser.ParseInteger() },
                { "RopeWidth", (parser, x) => x.RopeWidth = parser.ParseFloat() },
                { "RopeColor", (parser, x) => x.RopeColor = parser.ParseColorRgb() },
                { "RopeWobbleLen", (parser, x) => x.RopeWobbleLen = parser.ParseInteger() },
                { "RopeWobbleAmplitude", (parser, x) => x.RopeWobbleAmplitude = parser.ParseFloat() },
                { "RopeWobbleRate", (parser, x) => x.RopeWobbleRate = parser.ParseInteger() },
                { "RopeFinalHeight", (parser, x) => x.RopeFinalHeight = parser.ParseInteger() },
                { "RappelSpeed", (parser, x) => x.RappelSpeed = parser.ParseInteger() },
                { "MinDropHeight", (parser, x) => x.MinDropHeight = parser.ParseInteger() },
                { "UpgradedSupplyBoost", (parser, x) => x.UpgradedSupplyBoost = parser.ParseInteger() },
                { "RotorWashParticleSystem", (parser, x) => x.RotorWashParticleSystem = parser.ParseAssetReference() },
            });

        public int NumRopes { get; private set; }
        public int PerRopeDelayMin { get; private set; }
        public int PerRopeDelayMax { get; private set; }
        public float RopeWidth { get; private set; }
        public ColorRgb RopeColor { get; private set; }
        public int RopeWobbleLen { get; private set; }
        public float RopeWobbleAmplitude { get; private set; }
        public int RopeWobbleRate { get; private set; }
        public int RopeFinalHeight { get; private set; }
        public int RappelSpeed { get; private set; }
        public int MinDropHeight { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int UpgradedSupplyBoost { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string RotorWashParticleSystem { get; private set; }

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new ChinookAIUpdate(gameObject, this);
        }
    }
}
