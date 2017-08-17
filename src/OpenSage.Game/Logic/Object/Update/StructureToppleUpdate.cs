using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class StructureToppleUpdate : ObjectBehavior
    {
        internal static StructureToppleUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StructureToppleUpdate> FieldParseTable = new IniParseTable<StructureToppleUpdate>
        {
            { "MinToppleDelay", (parser, x) => x.MinToppleDelay = parser.ParseInteger() },
            { "MaxToppleDelay", (parser, x) => x.MaxToppleDelay = parser.ParseInteger() },
            { "MinToppleBurstDelay", (parser, x) => x.MinToppleBurstDelay = parser.ParseInteger() },
            { "MaxToppleBurstDelay", (parser, x) => x.MaxToppleBurstDelay = parser.ParseInteger() },
            { "StructuralIntegrity", (parser, x) => x.StructuralIntegrity = parser.ParseFloat() },
            { "StructuralDecay", (parser, x) => x.StructuralDecay = parser.ParseFloat() },
            { "DamageFXTypes", (parser, x) => x.DamageFXTypes = parser.ParseEnumBitArray<DamageType>() },
            { "ToppleStartFX", (parser, x) => x.ToppleStartFX = parser.ParseAssetReference() },
            { "ToppleDelayFX", (parser, x) => x.ToppleDelayFX = parser.ParseAssetReference() },
            { "CrushingFX", (parser, x) => x.CrushingFX = parser.ParseAssetReference() },
            { "AngleFX", (parser, x) => x.AngleFX = StructureToppleAngleFX.Parse(parser) },
            { "ToppleDoneFX", (parser, x) => x.ToppleDoneFX = parser.ParseAssetReference() },
            { "CrushingWeaponName", (parser, x) => x.CrushingWeaponName = parser.ParseAssetReference() },
        };

        public int MinToppleDelay { get; private set; }
        public int MaxToppleDelay { get; private set; }
        public int MinToppleBurstDelay { get; private set; }
        public int MaxToppleBurstDelay { get; private set; }
        public float StructuralIntegrity { get; private set; }
        public float StructuralDecay { get; private set; }
        public BitArray<DamageType> DamageFXTypes { get; private set; }
        public string ToppleStartFX { get; private set; }
        public string ToppleDelayFX { get; private set; }
        public string CrushingFX { get; private set; }
        public StructureToppleAngleFX AngleFX { get; private set; }
        public string ToppleDoneFX { get; private set; }
        public string CrushingWeaponName { get; private set; }
    }

    public struct StructureToppleAngleFX
    {
        internal static StructureToppleAngleFX Parse(IniParser parser)
        {
            return new StructureToppleAngleFX
            {
                Angle = parser.ParseFloat(),
                FX = parser.ParseAssetReference()
            };
        }

        public float Angle;
        public string FX;
    }
}
