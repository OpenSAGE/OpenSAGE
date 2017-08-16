using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ObjectConditionState
    {
        internal static ObjectConditionState ParseDefault(IniParser parser)
        {
            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = new BitArray<ModelConditionFlag>(); // "NONE"

            return result;
        }

        internal static ObjectConditionState Parse(IniParser parser)
        {
            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = conditionFlags;

            return result;
        }

        private static readonly IniParseTable<ObjectConditionState> FieldParseTable = new IniParseTable<ObjectConditionState>
        {
            { "Model", (parser, x) => x.Model = parser.ParseFileName() },
            { "Turret", (parser, x) => x.Turret = parser.ParseAssetReference() },
            { "Animation", (parser, x) => x.Animation = parser.ParseAnimationName() },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationSpeedFactorRange", (parser, x) => x.AnimationSpeedFactorRange = FloatRange.Parse(parser) },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnumFlags<ObjectConditionStateFlags>() },
            { "WeaponFireFXBone", (parser, x) => x.WeaponFireFXBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponRecoilBone", (parser, x) => x.WeaponRecoilBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponMuzzleFlash", (parser, x) => x.WeaponMuzzleFlashes.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponLaunchBone", (parser, x) => x.WeaponLaunchBones.Add(BoneAttachPoint.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
            { "HideSubObject", (parser, x) => x.HideSubObject = parser.ParseAssetReference() },
            { "ShowSubObject", (parser, x) => x.ShowSubObject = parser.ParseAssetReference() },
        };

        public BitArray<ModelConditionFlag> ConditionFlags { get; private set; }

        public string Model { get; private set; }
        public string Turret { get; private set; }
        public string Animation { get; private set; }
        public AnimationMode AnimationMode { get; private set; }
        public FloatRange AnimationSpeedFactorRange { get; private set; }
        public ObjectConditionStateFlags Flags { get; private set; }
        public List<BoneAttachPoint> WeaponFireFXBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponRecoilBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponMuzzleFlashes { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponLaunchBones { get; private set; } = new List<BoneAttachPoint>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();
        public string HideSubObject { get; private set; }
        public string ShowSubObject { get; private set; }

        public ObjectConditionState Clone(BitArray<ModelConditionFlag> conditionFlags)
        {
            return new ObjectConditionState
            {
                ConditionFlags = conditionFlags,

                Model = Model,
                Turret = Turret,
                Animation = Animation,
                AnimationMode = AnimationMode,
                Flags = Flags,
                ParticleSysBones = ParticleSysBones,
                HideSubObject = HideSubObject,
                ShowSubObject = ShowSubObject
            };
        }
    }

    public sealed class BoneAttachPoint
    {
        internal static BoneAttachPoint Parse(IniParser parser)
        {
            return new BoneAttachPoint
            {
                WeaponSlot = parser.ParseEnum<WeaponSlot>(),
                BoneName = parser.ParseBoneName()
            };
        }

        public WeaponSlot WeaponSlot { get; private set; }
        public string BoneName { get; private set; }
    }

    public sealed class ParticleSysBone
    {
        internal static ParticleSysBone Parse(IniParser parser)
        {
            return new ParticleSysBone
            {
                BoneName = parser.ParseBoneName(),
                ParticleSystem = parser.ParseAssetReference()
            };
        }

        public string BoneName { get; private set; }
        public string ParticleSystem { get; private set; }
    }
}
