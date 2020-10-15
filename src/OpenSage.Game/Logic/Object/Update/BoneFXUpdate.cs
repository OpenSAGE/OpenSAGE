using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class BoneFXUpdateModuleData : UpdateModuleData
    {
        internal static BoneFXUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXUpdateModuleData> FieldParseTable = new IniParseTable<BoneFXUpdateModuleData>
        {
            { "DamageFXTypes", (parser, x) => x.DamageFXTypes = parser.ParseEnumBitArray<DamageType>() },
            { "RubbleFXList1", (parser, x) => x.RubbleFXList1 = BoneFXUpdateFXList.Parse(parser) },

            { "DamageParticleTypes", (parser, x) => x.DamageParticleTypes = parser.ParseEnumBitArray<DamageType>() },
            { "PristineParticleSystem1", (parser, x) => x.PristineParticleSystem1 = BoneFXUpdateParticleSystem.Parse(parser) },
            { "RubbleParticleSystem1", (parser, x) => x.RubbleParticleSystem1 = BoneFXUpdateParticleSystem.Parse(parser) },

            { "PristineParticleSystem2", (parser, x) => x.PristineParticleSystem2 = BoneFXUpdateParticleSystem.Parse(parser) },

            { "PristineParticleSystem3", (parser, x) => x.PristineParticleSystem3 = BoneFXUpdateParticleSystem.Parse(parser) },

            { "PristineParticleSystem4", (parser, x) => x.PristineParticleSystem4 = BoneFXUpdateParticleSystem.Parse(parser) },

            { "PristineParticleSystem5", (parser, x) => x.PristineParticleSystem5 = BoneFXUpdateParticleSystem.Parse(parser) },

            { "PristineParticleSystem6", (parser, x) => x.PristineParticleSystem6 = BoneFXUpdateParticleSystem.Parse(parser) },
        };

        public BitArray<DamageType> DamageFXTypes { get; private set; }
        public BoneFXUpdateFXList RubbleFXList1 { get; private set; }

        public BitArray<DamageType> DamageParticleTypes { get; private set; }
        public BoneFXUpdateParticleSystem PristineParticleSystem1 { get; private set; }
        public BoneFXUpdateParticleSystem RubbleParticleSystem1 { get; private set; }

        public BoneFXUpdateParticleSystem PristineParticleSystem2 { get; private set; }

        public BoneFXUpdateParticleSystem PristineParticleSystem3 { get; private set; }

        public BoneFXUpdateParticleSystem PristineParticleSystem4 { get; private set; }

        public BoneFXUpdateParticleSystem PristineParticleSystem5 { get; private set; }

        public BoneFXUpdateParticleSystem PristineParticleSystem6 { get; private set; }
    }

    public sealed class BoneFXUpdateFXList
    {
        internal static BoneFXUpdateFXList Parse(IniParser parser)
        {
            return new BoneFXUpdateFXList
            {
                Bone = parser.ParseAttribute("Bone", parser.ScanBoneName),
                OnlyOnce = parser.ParseAttributeBoolean("OnlyOnce"),
                Min = parser.ParseInteger(),
                Max = parser.ParseInteger(),
                FXList = parser.ParseAttribute("FXList", parser.ScanAssetReference)
            };
        }

        public string Bone { get; private set; }
        public bool OnlyOnce { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public string FXList { get; private set; }
    }

    public sealed class BoneFXUpdateParticleSystem
    {
        internal static BoneFXUpdateParticleSystem Parse(IniParser parser)
        {
            return new BoneFXUpdateParticleSystem
            {
                Bone = parser.ParseAttribute("Bone", parser.ScanBoneName),
                OnlyOnce = parser.ParseAttributeBoolean("OnlyOnce"),
                Min = parser.ParseInteger(),
                Max = parser.ParseInteger(),
                ParticleSystem = parser.ParseAttribute("PSys", parser.ScanAssetReference)
            };
        }

        public string Bone { get; private set; }
        public bool OnlyOnce { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public string ParticleSystem { get; private set; }
    }
}
