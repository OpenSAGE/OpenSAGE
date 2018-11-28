using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class TransitionDamageFXModuleData : DamageModuleData
    {
        internal static TransitionDamageFXModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TransitionDamageFXModuleData> FieldParseTable = new IniParseTable<TransitionDamageFXModuleData>
        {
            { "DamageFXTypes", (parser, x) => x.DamageFXTypes = parser.ParseEnumBitArray<DamageType>() },

            { "DamagedFXList1", (parser, x) => x.DamagedFXList1 = TransitionDamageFXList.Parse(parser) },

            { "ReallyDamagedFXList1", (parser, x) => x.ReallyDamagedFXList1 = TransitionDamageFXList.Parse(parser) },

            { "RubbleFXList1", (parser, x) => x.RubbleFXList1 = TransitionDamageFXList.Parse(parser) },

            { "DamageParticleTypes", (parser, x) => x.DamageParticleTypes = parser.ParseEnumBitArray<DamageType>() },

            { "DamagedParticleSystem1", (parser, x) => x.DamagedParticleSystem1 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem2", (parser, x) => x.DamagedParticleSystem2 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem3", (parser, x) => x.DamagedParticleSystem3 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem4", (parser, x) => x.DamagedParticleSystem4 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem5", (parser, x) => x.DamagedParticleSystem5 = TransitionDamageParticleSystem.Parse(parser) },
            { "DamagedParticleSystem6", (parser, x) => x.DamagedParticleSystem6 = TransitionDamageParticleSystem.Parse(parser) },

            { "ReallyDamagedParticleSystem1", (parser, x) => x.ReallyDamagedParticleSystem1 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem2", (parser, x) => x.ReallyDamagedParticleSystem2 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem3", (parser, x) => x.ReallyDamagedParticleSystem3 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem4", (parser, x) => x.ReallyDamagedParticleSystem4 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem5", (parser, x) => x.ReallyDamagedParticleSystem5 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem6", (parser, x) => x.ReallyDamagedParticleSystem6 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem7", (parser, x) => x.ReallyDamagedParticleSystem7 = TransitionDamageParticleSystem.Parse(parser) },
            { "ReallyDamagedParticleSystem8", (parser, x) => x.ReallyDamagedParticleSystem8 = TransitionDamageParticleSystem.Parse(parser) },

            { "RubbleParticleSystem1", (parser, x) => x.RubbleParticleSystem1 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem2", (parser, x) => x.RubbleParticleSystem2 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem3", (parser, x) => x.RubbleParticleSystem3 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem4", (parser, x) => x.RubbleParticleSystem4 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem5", (parser, x) => x.RubbleParticleSystem5 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem6", (parser, x) => x.RubbleParticleSystem6 = TransitionDamageParticleSystem.Parse(parser) },
            { "RubbleParticleSystem7", (parser, x) => x.RubbleParticleSystem7 = TransitionDamageParticleSystem.Parse(parser) },

            { "RubbleNeighbor", (parser, x) => x.RubbleNeighbors.Add(RubbleNeighbor.Parse(parser)) },
            { "PristineShowSubObject", (parser, x) => x.PristineShowSubObject = parser.ParseAssetReferenceArray() },
            { "PristineHideSubObject", (parser, x) => x.PristineHideSubObject = parser.ParseAssetReferenceArray() },
            { "DamagedShowSubObject", (parser, x) => x.DamagedShowSubObject = parser.ParseAssetReferenceArray() },
            { "DamagedHideSubObject", (parser, x) => x.DamagedHideSubObject = parser.ParseAssetReferenceArray() },
            { "ReallyDamagedHideSubObject", (parser, x) => x.ReallyDamagedHideSubObject = parser.ParseAssetReferenceArray() },
            { "ReallyDamagedShowSubObject", (parser, x) => x.ReallyDamagedShowSubObject = parser.ParseAssetReferenceArray() },
        };

        public BitArray<DamageType> DamageFXTypes { get; private set; }

        public TransitionDamageFXList DamagedFXList1 { get; private set; }

        public TransitionDamageFXList ReallyDamagedFXList1 { get; private set; }

        public TransitionDamageFXList RubbleFXList1 { get; private set; }

        public BitArray<DamageType> DamageParticleTypes { get; private set; }

        public TransitionDamageParticleSystem DamagedParticleSystem1 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem2 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem3 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem4 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem5 { get; private set; }
        public TransitionDamageParticleSystem DamagedParticleSystem6 { get; private set; }

        public TransitionDamageParticleSystem ReallyDamagedParticleSystem1 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem2 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem3 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem4 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem5 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem6 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem7 { get; private set; }
        public TransitionDamageParticleSystem ReallyDamagedParticleSystem8 { get; private set; }

        public TransitionDamageParticleSystem RubbleParticleSystem1 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem2 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem3 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem4 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem5 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem6 { get; private set; }
        public TransitionDamageParticleSystem RubbleParticleSystem7 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<RubbleNeighbor> RubbleNeighbors { get; private set; } = new List<RubbleNeighbor>();

        [AddedIn(SageGame.Bfme)]
        public string[] PristineShowSubObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] PristineHideSubObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] DamagedShowSubObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] DamagedHideSubObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] ReallyDamagedShowSubObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] ReallyDamagedHideSubObject { get; private set; }
    }

    public sealed class TransitionDamageFXList
    {
        internal static TransitionDamageFXList Parse(IniParser parser)
        {
            return new TransitionDamageFXList
            {
                Location = parser.ParseAttribute("Loc", () => Coord3D.Parse(parser)),
                FXList = parser.ParseAttribute("FXList", parser.ScanAssetReference)
            };
        }

        public Coord3D Location { get; private set; }
        public string FXList { get; private set; }
    }

    public sealed class TransitionDamageParticleSystem
    {
        internal static TransitionDamageParticleSystem Parse(IniParser parser)
        {
            return new TransitionDamageParticleSystem
            {
                Bone = parser.ParseAttribute("Bone", parser.ScanBoneName),
                RandomBone = parser.ParseAttributeBoolean("RandomBone"),
                ParticleSystem = parser.ParseAttribute("PSys", parser.ScanAssetReference)
            };
        }

        public string Bone { get; private set; }
        public bool RandomBone { get; private set; }
        public string ParticleSystem { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class RubbleNeighbor
    {
        internal static RubbleNeighbor Parse(IniParser parser)
        {
            var result = new RubbleNeighbor();
            result.Offset = parser.ParseAttributeVector3("NeighborOffset");
            result.SubObjects.Add(parser.ParseAttributeIdentifier("SubObject"));
            result.SubObjects.Add(parser.ParseAttributeIdentifier("SubObject"));
            result.OCL = parser.ParseAttributeIdentifier("OCL");
            return result;
        }

        public Vector3 Offset { get; private set; }
        public List<string> SubObjects { get; }= new List<string>();
        public string OCL { get; private set; }
    }
}
