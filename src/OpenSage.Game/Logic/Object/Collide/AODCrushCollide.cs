using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AODCrushCollideModuleData : CollideModuleData
    {
        internal static AODCrushCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AODCrushCollideModuleData> FieldParseTable = new IniParseTable<AODCrushCollideModuleData>
            {
                { "SmallFXList", (parser, x) => x.SmallFXList = parser.ParseAssetReference() },
                { "MediumFXList", (parser, x) => x.MediumFXList = parser.ParseAssetReference() },
                { "LargeFXList", (parser, x) => x.LargeFXList = parser.ParseAssetReference() },
                { "MediumObjectCreationList", (parser, x) => x.MediumObjectCreationList = parser.ParseAssetReference() },
                { "Damage", (parser, x) => x.Damage = parser.ParseFloat() },
                { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() },
                { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() },
                { "SpecialObject", (parser, x) => x.SpecialObject = ObjectFilter.Parse(parser) },
                { "SpecialDamage", (parser, x) => x.SpecialDamage = parser.ParseFloat() },
                { "SpecialDamageType", (parser, x) => x.SpecialDamageType = parser.ParseEnum<DamageType>() },
                { "SpecialDeathType", (parser, x) => x.SpecialDeathType = parser.ParseEnum<DeathType>() },
                { "SelfDamage", (parser, x) => x.SelfDamage = parser.ParseFloat() },
                { "SelfDamageType", (parser, x) => x.SelfDamageType = parser.ParseEnum<DamageType>() },
                { "SelfDeathType", (parser, x) => x.SelfDeathType = parser.ParseEnum<DeathType>() }
            };

        public string SmallFXList { get; private set; }
        public string MediumFXList { get; private set; }
        public string LargeFXList { get; private set; }
        public string MediumObjectCreationList { get; private set; }
        public float Damage { get; private set; }
        public DamageType DamageType { get; private set; }
        public DeathType DeathType { get; private set; }
        public ObjectFilter SpecialObject { get; private set; }
        public float SpecialDamage { get; private set; }
        public DamageType SpecialDamageType { get; private set; }
        public DeathType SpecialDeathType { get; private set; }
        public float SelfDamage { get; private set; }
        public DamageType SelfDamageType { get; private set; }
        public DeathType SelfDeathType { get; private set; }
    }
}
