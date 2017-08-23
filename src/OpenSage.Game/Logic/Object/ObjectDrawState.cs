using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ObjectDrawState
    {
        internal static readonly IniParseTable<ObjectDrawState> BaseFieldParseTable = new IniParseTable<ObjectDrawState>
        {
            { "Model", (parser, x) => x.Model = parser.ParseFileName() },

            { "WeaponRecoilBone", (parser, x) => x.WeaponRecoilBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponFireFXBone", (parser, x) => x.WeaponFireFXBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponMuzzleFlash", (parser, x) => x.WeaponMuzzleFlashes.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponLaunchBone", (parser, x) => x.WeaponLaunchBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponHideShowBone", (parser, x) => x.WeaponHideShowBones.Add(BoneAttachPoint.Parse(parser)) },

            { "Animation", (parser, x) => x.Animations.Add(ObjectConditionAnimation.Parse(parser)) },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationSpeedFactorRange", (parser, x) => x.AnimationSpeedFactorRange = FloatRange.Parse(parser) },
            { "IdleAnimation", (parser, x) => x.IdleAnimations.Add(ObjectConditionAnimation.Parse(parser)) },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnumFlags<AnimationFlags>() },

            { "Turret", (parser, x) => x.Turret = parser.ParseAssetReference() },
            { "TurretArtAngle", (parser, x) => x.TurretArtAngle = parser.ParseInteger() },
            { "TurretPitch", (parser, x) => x.TurretPitch = parser.ParseAssetReference() },
            { "AltTurret", (parser, x) => x.AltTurret = parser.ParseAssetReference() },
            { "AltTurretPitch", (parser, x) => x.AltTurretPitch = parser.ParseAssetReference() },

            { "HideSubObject", (parser, x) => x.HideSubObject = parser.ParseAssetReferenceArray() },
            { "ShowSubObject", (parser, x) => x.ShowSubObject = parser.ParseAssetReferenceArray() },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
        };

        public string Model { get; protected set; }

        // Weapon bone settings
        public List<BoneAttachPoint> WeaponRecoilBones { get; protected set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponFireFXBones { get; protected set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponMuzzleFlashes { get; protected set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponLaunchBones { get; protected set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponHideShowBones { get; protected set; } = new List<BoneAttachPoint>();

        // Model animation settings
        public List<ObjectConditionAnimation> Animations { get; protected set; } = new List<ObjectConditionAnimation>();
        public AnimationMode AnimationMode { get; protected set; }
        public FloatRange AnimationSpeedFactorRange { get; protected set; }
        public List<ObjectConditionAnimation> IdleAnimations { get; protected set; } = new List<ObjectConditionAnimation>();
        public AnimationFlags Flags { get; protected set; }

        // Turret settings
        public string Turret { get; protected set; }
        public int TurretArtAngle { get; protected set; }
        public string TurretPitch { get; protected set; }
        public string AltTurret { get; protected set; }
        public string AltTurretPitch { get; protected set; }

        // Misc settings
        public string[] HideSubObject { get; protected set; }
        public string[] ShowSubObject { get; protected set; }
        public List<ParticleSysBone> ParticleSysBones { get; protected set; } = new List<ParticleSysBone>();
    }

    public sealed class ObjectConditionAnimation
    {
        internal static ObjectConditionAnimation Parse(IniParser parser)
        {
            var result = new ObjectConditionAnimation
            {
                Animation = parser.ParseAnimationName()
            };

            if (parser.Current.TokenType == IniTokenType.IntegerLiteral
                || parser.Current.TokenType == IniTokenType.FloatLiteral)
            {
                result.Unknown1 = parser.ParseFloat();

                if (parser.Current.TokenType == IniTokenType.IntegerLiteral)
                {
                    result.Unknown2 = parser.ParseInteger();
                }
            }

            return result;
        }

        public string Animation { get; private set; }
        public float Unknown1 { get; private set; }
        public int Unknown2 { get; private set; }
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
