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
           { "MorphCondition", (parser, x) => x.MorphCondition = MorphCondition.Parse(parser) }
        };

        public int IdleSpawnRate { get; private set; }
        public int MeleeFreeUnitSpawnTime { get; private set; }
        public int DiedRespawnTime { get; private set; }
        public int MeleeFreeBannerReSpawnTime { get; private set; }
        public string BannerMorphFX { get; private set; }
        public string UnitSpawnFX { get; private set; }
        public MorphCondition MorphCondition { get; private set; }
    }

    public sealed class MorphCondition
    {
        internal static MorphCondition Parse(IniParser parser)
        {
            var result  = new MorphCondition();
            result.UnitType = parser.ParseAttributeIdentifier("UnitType");

            //TODO: find a way to parse this shit

            //MorphCondition		= UnitType:IsengardWargRider		Locomotor:SET_MOUNTED	ModelState:"USER_2 MOUNTED"		
            //MorphCondition		= UnitType:MordorHaradrimArcher			ModelState:"USER_2"
            parser.GetNextTokenOptional();
            parser.GetNextTokenOptional();
            return result;
        }

        public string UnitType { get; private set; }
        public LocomotorSetCondition Locomotor { get; private set; }
        public BitArray<ModelConditionFlag> ModelStates { get; private set; }
    }
}
