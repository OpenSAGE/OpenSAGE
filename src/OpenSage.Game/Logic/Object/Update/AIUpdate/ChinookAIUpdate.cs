using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class ChinookAIUpdate : SupplyTruckAIUpdate
    {
        private readonly ChinookAIUpdateModuleData _moduleData;

        internal ChinookAIUpdate(GameObject gameObject, ChinookAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        protected override int GetAdditionalValuePerSupplyBox(ScopedAssetCollection<UpgradeTemplate> upgrades)
        {
            // this is also hardcoded in original SAGE, replaced by BonusScience and BonusScienceMultiplier (SupplyCenterDockUpdate) in later games
            var upgradeDefinition = upgrades.GetByName("Upgrade_AmericaSupplyLines");
            return GameObject.HasUpgrade(upgradeDefinition) ? _moduleData.UpgradedSupplyBoost : 0;
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.SkipUnknownBytes(1);

            var unknown2 = true;
            reader.ReadBoolean(ref unknown2);
            if (!unknown2)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(7);
        }
    }

    /// <summary>
    /// Logic requires bones for either end of the rope to be defined as RopeEnd and RopeStart.
    /// Infantry (or tanks) can be made to rappel down a rope by adding CAN_RAPPEL to the object's 
    /// KindOf field. Having done that, the "RAPPELLING" ModelConditionState becomes available for 
    /// rappelling out of the object that has the rappel code of this module.
    /// </summary>
    public sealed class ChinookAIUpdateModuleData : SupplyTruckAIUpdateModuleData
    {
        internal new static ChinookAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<ChinookAIUpdateModuleData> FieldParseTable = SupplyTruckAIUpdateModuleData.FieldParseTable
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
