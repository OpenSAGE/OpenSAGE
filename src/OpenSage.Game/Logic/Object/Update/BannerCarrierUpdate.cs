using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class BannerCarrierUpdateModuleData : UpdateModuleData
    {
        internal static BannerCarrierUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BannerCarrierUpdateModuleData> FieldParseTable = new IniParseTable<BannerCarrierUpdateModuleData>
        {
           { "IdleSpawnRate", (parser, x) => x.IdleSpawnRate = parser.ParseInteger() },
           { "MeleeFreeUnitSpawnTime", (parser, x) => x.MeleeFreeUnitSpawnTime = parser.ParseInteger() },
           { "DiedRespawnTime", (parser, x) => x.DiedRespawnTime = parser.ParseInteger() },
           { "MeleeFreeBannerReSpawnTime", (parser, x) => x.MeleeFreeBannerReSpawnTime = parser.ParseInteger() },
           { "BannerMorphFX", (parser, x) => x.BannerMorphFX = parser.ParseAssetReference() },
           { "UnitSpawnFX", (parser, x) => x.UnitSpawnFX = parser.ParseAssetReference() },
           { "MorphCondition", (parser, x) => x.MorphCondition = MorphCondition.Parse(parser) },
           { "ReplenishNearbyHorde", (parser, x) => x.ReplenishNearbyHorde = parser.ParseBoolean() },
           { "ScanHordeDistance", (parser, x) => x.ScanHordeDistance = parser.ParseFloat() },
           { "ReplenishAllNearbyHordes", (parser, x) => x.ReplenishAllNearbyHordes = parser.ParseBoolean() },
        };

        public int IdleSpawnRate { get; private set; }
        public int MeleeFreeUnitSpawnTime { get; private set; }
        public int DiedRespawnTime { get; private set; }
        public int MeleeFreeBannerReSpawnTime { get; private set; }
        public string BannerMorphFX { get; private set; }
        public string UnitSpawnFX { get; private set; }
        public MorphCondition MorphCondition { get; private set; }
        public bool ReplenishNearbyHorde { get; private set; }
        public float ScanHordeDistance { get; private set; }
        public bool ReplenishAllNearbyHordes { get; private set; }
    }

    public sealed class MorphCondition
    {
        internal static MorphCondition Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<MorphCondition> FieldParseTable = new IniParseTable<MorphCondition>
        {
            { "UnitType", (parser, x) => x.UnitType = parser.ParseIdentifier() },
            { "Locomotor", (parser, x) => x.Locomotor = parser.ParseEnum<LocomotorSetCondition>() },
            { "ModelState", (parser, x) => x.ModelStates = parser.ParseEnumBitArray<ModelConditionFlag>(parser.ParseString()) }
        };

        public string UnitType { get; private set; }
        public LocomotorSetCondition Locomotor { get; private set; }
        public BitArray<ModelConditionFlag> ModelStates { get; private set; }
    }
}
