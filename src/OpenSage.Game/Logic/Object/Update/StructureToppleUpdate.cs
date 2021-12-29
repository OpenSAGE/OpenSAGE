using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class StructureToppleUpdate : UpdateModule
    {
        private float _unknownFloat;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.SkipUnknownBytes(20);

            _unknownFloat = reader.ReadSingle();

            reader.SkipUnknownBytes(8);

            var unknownInt1 = -1;
            reader.ReadInt32(ref unknownInt1);
            if (unknownInt1 != -1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(12);
        }
    }

    public sealed class StructureToppleUpdateModuleData : UpdateModuleData
    {
        internal static StructureToppleUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StructureToppleUpdateModuleData> FieldParseTable = new IniParseTable<StructureToppleUpdateModuleData>
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
            { "ToppleAccelerationFactor", (parser, x) => x.ToppleAccelerationFactor = parser.ParseFloat() },
            { "ForceToppleAngle", (parser, x) => x.ForceToppleAngle = parser.ParseInteger() },
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

        [AddedIn(SageGame.Bfme)]
        public float ToppleAccelerationFactor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ForceToppleAngle { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new StructureToppleUpdate();
        }
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
