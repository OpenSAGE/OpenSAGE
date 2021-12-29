using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.FX;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class TransitionDamageFX : DamageModule
    {
        private readonly TransitionDamageFXModuleData _moduleData;

        private const int NumParticleSystemsPerDamageType = 12;
        private readonly uint[] _particleSystemIds = new uint[EnumUtility.GetEnumCount<BodyDamageType>() * NumParticleSystemsPerDamageType];

        public TransitionDamageFX(TransitionDamageFXModuleData moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void OnDamageStateChanged(BehaviorUpdateContext context, BodyDamageType fromDamage, BodyDamageType toDamage)
        {
            List<TransitionDamageParticleSystem> particleSystems = null;

            switch ((fromDamage, toDamage))
            {
                case (BodyDamageType.Pristine, BodyDamageType.Damaged):
                    particleSystems = _moduleData.DamagedParticleSystems;
                    break;

                case (BodyDamageType.Damaged, BodyDamageType.ReallyDamaged):
                    particleSystems = _moduleData.ReallyDamagedParticleSystems;
                    break;

                case (BodyDamageType.ReallyDamaged, BodyDamageType.Rubble):
                    particleSystems = _moduleData.RubbleParticleSystems;
                    break;
            }

            if (particleSystems != null)
            {
                var worldMatrix = context.GameObject.TransformMatrix;

                foreach (var particleSystemTemplate in particleSystems)
                {
                    context.GameContext.ParticleSystems
                        .Create(particleSystemTemplate.ParticleSystem.Value, worldMatrix)
                        .Activate();
                }
            }

            // TODO: FXLists
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            for (var i = 0; i < _particleSystemIds.Length; i++)
            {
                reader.ReadUInt32(ref _particleSystemIds[i]);
            }
        }
    }

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

            { "DamagedParticleSystem1", (parser, x) => x.DamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "DamagedParticleSystem2", (parser, x) => x.DamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "DamagedParticleSystem3", (parser, x) => x.DamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "DamagedParticleSystem4", (parser, x) => x.DamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "DamagedParticleSystem5", (parser, x) => x.DamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "DamagedParticleSystem6", (parser, x) => x.DamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },

            { "ReallyDamagedParticleSystem1", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "ReallyDamagedParticleSystem2", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "ReallyDamagedParticleSystem3", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "ReallyDamagedParticleSystem4", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "ReallyDamagedParticleSystem5", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "ReallyDamagedParticleSystem6", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "ReallyDamagedParticleSystem7", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "ReallyDamagedParticleSystem8", (parser, x) => x.ReallyDamagedParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },

            { "RubbleParticleSystem1", (parser, x) => x.RubbleParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "RubbleParticleSystem2", (parser, x) => x.RubbleParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "RubbleParticleSystem3", (parser, x) => x.RubbleParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "RubbleParticleSystem4", (parser, x) => x.RubbleParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "RubbleParticleSystem5", (parser, x) => x.RubbleParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "RubbleParticleSystem6", (parser, x) => x.RubbleParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },
            { "RubbleParticleSystem7", (parser, x) => x.RubbleParticleSystems.Add(TransitionDamageParticleSystem.Parse(parser)) },

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

        public List<TransitionDamageParticleSystem> DamagedParticleSystems { get; } = new List<TransitionDamageParticleSystem>();

        public List<TransitionDamageParticleSystem> ReallyDamagedParticleSystems { get; } = new List<TransitionDamageParticleSystem>();

        public List<TransitionDamageParticleSystem> RubbleParticleSystems { get; } = new List<TransitionDamageParticleSystem>();

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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new TransitionDamageFX(this);
        }
    }

    public sealed class TransitionDamageFXList
    {
        internal static TransitionDamageFXList Parse(IniParser parser)
        {
            return new TransitionDamageFXList
            {
                Location = parser.ParseAttribute("Loc", () => parser.ParseVector3()),
                FXList = parser.ParseAttribute("FXList", parser.ScanFXListReference)
            };
        }

        public Vector3 Location { get; private set; }
        public LazyAssetReference<FXList> FXList { get; private set; }
    }

    public sealed class TransitionDamageParticleSystem
    {
        internal static TransitionDamageParticleSystem Parse(IniParser parser)
        {
            return new TransitionDamageParticleSystem
            {
                Bone = parser.ParseAttribute("Bone", parser.ScanBoneName),
                RandomBone = parser.ParseAttributeBoolean("RandomBone"),
                ParticleSystem = parser.ParseAttribute("PSys", parser.ScanFXParticleSystemTemplateReference)
            };
        }

        public string Bone { get; private set; }
        public bool RandomBone { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> ParticleSystem { get; private set; }
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
