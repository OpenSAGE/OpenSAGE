using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class ModelConditionState : IConditionState
    {
        internal static ModelConditionState ParseDefault(IniParser parser)
        {
            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = new BitArray<ModelConditionFlag>(); // "NONE"

            return result;
        }

        internal static ModelConditionState Parse(IniParser parser, ModelConditionState defaultConditionState)
        {
            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var result = parser.ParseBlock(FieldParseTable, defaultConditionState?.Clone() ?? new ModelConditionState());

            result.ConditionFlags = conditionFlags;

            return result;
        }

        internal static readonly IniParseTable<ModelConditionState> FieldParseTable = new IniParseTable<ModelConditionState>
        {
            { "Model", (parser, x) => x.Model = parser.ParseModelReference() },
            { "Skeleton", (parser, x) => x.Skeleton = parser.ParseFileName() },

            { "WeaponRecoilBone", (parser, x) => x.WeaponRecoilBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponFireFXBone", (parser, x) => x.WeaponFireFXBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponMuzzleFlash", (parser, x) => x.WeaponMuzzleFlashes.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponLaunchBone", (parser, x) => x.WeaponLaunchBones.Add(BoneAttachPoint.Parse(parser)) },
            { "WeaponHideShowBone", (parser, x) => x.WeaponHideShowBones.Add(BoneAttachPoint.Parse(parser)) },
            { "Animation", (parser, x) => x.ParseAnimation(parser) },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationSpeedFactorRange", (parser, x) => x.AnimationSpeedFactorRange = parser.ParseFloatRange() },
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

            { "TransitionKey", (parser, x) => x.TransitionKey = parser.ParseIdentifier() },
            { "WaitForStateToFinishIfPossible", (parser, x) => x.WaitForStateToFinishIfPossible = parser.ParseIdentifier() },

            { "OverrideTooltip", (parser, x) => x.OverrideTooltip = parser.ParseAssetReference() },
            { "FXEvent", (parser, x) => x.FXEvents.Add(FXEvent.Parse(parser)) },

            { "Shadow", (parser, x) => x.Shadow = parser.ParseEnum<ObjectShadowType>() },
            { "ShadowSizeX", (parser, x) => x.ShadowSizeX = parser.ParseInteger() },
            { "ShadowSizeY", (parser, x) => x.ShadowSizeY = parser.ParseInteger() },
            { "ShadowTexture", (parser, x) => x.ShadowTexture = parser.ParseAssetReference() },
            { "ShadowMaxHeight", (parser, x) => x.ShadowMaxHeight = parser.ParseInteger() },
            { "ShadowOpacityStart", (parser, x) => x.ShadowOpacityStart = parser.ParseInteger() },
            { "ShadowOpacityFadeInTime", (parser, x) => x.ShadowOpacityFadeInTime = parser.ParseInteger() },
            { "ShadowOpacityPeak", (parser, x) => x.ShadowOpacityPeak = parser.ParseInteger() },
            { "ShadowOpacityFadeOutTime", (parser, x) => x.ShadowOpacityFadeOutTime = parser.ParseInteger() },
            { "ShadowOpacityEnd", (parser, x) => x.ShadowOpacityEnd = parser.ParseInteger() },
            { "ShadowOverrideLODVisibility", (parser, x) => x.ShadowOverrideLODVisibility = parser.ParseBoolean() },
            { "StateName", (parser, x) => x.StateName = parser.ParseString() },
            { "RetainSubObjects", (parser, x) => x.RetainSubObjects = parser.ParseBoolean() },
            { "Texture", (parser, x) => x.Textures = parser.ParseAssetReferenceArray() },
            { "ModelAnimationPrefix", (parser, x) => x.ModelAnimationPrefix = parser.ParseString() },
            { "PortraitImageName", (parser, x) => x.PortraitImageName = parser.ParseString() },
            { "ButtonImageName", (parser, x) => x.ButtonImageName = parser.ParseString() },
        };

        private void ParseAnimation(IniParser parser)
        {
            if (parser.SageGame == SageGame.CncGenerals || parser.SageGame == SageGame.CncGeneralsZeroHour)
            {
                ConditionAnimations.Add(ObjectConditionAnimation.Parse(parser));
            }
            else
            {
                Animations.Add(AnimationStateAnimation.Parse(parser));
            }
        }

        internal ModelConditionState Clone()
        {
            var result = (ModelConditionState) MemberwiseClone();

            result.ConditionFlags = new BitArray<ModelConditionFlag>(result.ConditionFlags);
            result.WeaponRecoilBones = new List<BoneAttachPoint>(result.WeaponRecoilBones);
            result.WeaponFireFXBones = new List<BoneAttachPoint>(result.WeaponFireFXBones);
            result.WeaponMuzzleFlashes = new List<BoneAttachPoint>(result.WeaponMuzzleFlashes);
            result.WeaponLaunchBones = new List<BoneAttachPoint>(result.WeaponLaunchBones);
            result.WeaponHideShowBones = new List<BoneAttachPoint>(result.WeaponHideShowBones);

            result.ConditionAnimations = new List<ObjectConditionAnimation>(result.ConditionAnimations);
            result.Animations = new List<AnimationStateAnimation>(result.Animations);

            result.IdleAnimations = new List<ObjectConditionAnimation>(result.IdleAnimations);
            result.ParticleSysBones = new List<ParticleSysBone>(result.ParticleSysBones);
            result.FXEvents = new List<FXEvent>(result.FXEvents);
            return result;
        }

        public BitArray<ModelConditionFlag> ConditionFlags { get; private set; }

        public LazyAssetReference<Model> Model { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Skeleton { get; private set; }

        // Weapon bone settings
        public List<BoneAttachPoint> WeaponRecoilBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponFireFXBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponMuzzleFlashes { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponLaunchBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponHideShowBones { get; private set; } = new List<BoneAttachPoint>();

        // Model animation settings
        public List<ObjectConditionAnimation> ConditionAnimations { get; private set; } = new List<ObjectConditionAnimation>();
        public List<AnimationStateAnimation> Animations { get; private set; } = new List<AnimationStateAnimation>();
        public AnimationMode AnimationMode { get; private set; }
        public FloatRange AnimationSpeedFactorRange { get; private set; } = new FloatRange(1.0f, 1.0f);
        public List<ObjectConditionAnimation> IdleAnimations { get; private set; } = new List<ObjectConditionAnimation>();
        public AnimationFlags Flags { get; private set; }

        // Turret settings
        public string Turret { get; private set; }
        public int TurretArtAngle { get; private set; }
        public string TurretPitch { get; private set; }
        public string AltTurret { get; private set; }
        public string AltTurretPitch { get; private set; }

        // Misc settings
        public string[] HideSubObject { get; private set; }
        public string[] ShowSubObject { get; private set; }
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();

        public string TransitionKey { get; private set; }
        public string WaitForStateToFinishIfPossible { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string OverrideTooltip { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<FXEvent> FXEvents { get; private set; } = new List<FXEvent>();

        [AddedIn(SageGame.Bfme)]
        public ObjectShadowType Shadow { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowSizeX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowSizeY { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ShadowTexture { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowMaxHeight { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityStart { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityFadeInTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityPeak { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityFadeOutTime	{ get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShadowOpacityEnd { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShadowOverrideLODVisibility { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string StateName { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RetainSubObjects { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] Textures { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ModelAnimationPrefix { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string PortraitImageName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ButtonImageName { get; private set; }


        /// <summary>
        /// Used by AliasConditionState.
        /// </summary>
        public ModelConditionState Clone(BitArray<ModelConditionFlag> conditionFlags)
        {
            return new ModelConditionState
            {
                ConditionFlags = conditionFlags,

                Model = Model,

                WeaponRecoilBones = WeaponRecoilBones,
                WeaponFireFXBones = WeaponFireFXBones,
                WeaponMuzzleFlashes = WeaponMuzzleFlashes,
                WeaponLaunchBones = WeaponLaunchBones,
                WeaponHideShowBones = WeaponHideShowBones,

                ConditionAnimations = ConditionAnimations,
                AnimationMode = AnimationMode,
                AnimationSpeedFactorRange = AnimationSpeedFactorRange,
                IdleAnimations = IdleAnimations,
                TransitionKey = TransitionKey,
                WaitForStateToFinishIfPossible = WaitForStateToFinishIfPossible,
                Flags = Flags,

                Turret = Turret,
                TurretArtAngle = TurretArtAngle,
                TurretPitch = TurretPitch,
                AltTurret = AltTurret,
                AltTurretPitch = AltTurretPitch,

                HideSubObject = HideSubObject,
                ShowSubObject = ShowSubObject,
                ParticleSysBones = ParticleSysBones,
            };
        }
    }

    public sealed class ObjectConditionAnimation
    {
        internal static ObjectConditionAnimation Parse(IniParser parser)
        {
            var result = new ObjectConditionAnimation
            {
                Animation = parser.ParseAnimationReference()
            };


            var unknown1Token = parser.GetNextTokenOptional();

            if (unknown1Token != null)
            {
                result.Unknown1 = parser.ScanFloat(unknown1Token.Value);

                var unknown2Token = parser.GetNextTokenOptional();
                if (unknown2Token != null)
                {
                    result.Unknown2 = parser.ScanInteger(unknown2Token.Value);
                }
            }

            return result;
        }

        public LazyAssetReference<Graphics.Animation.W3DAnimation> Animation { get; private set; }
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
                ParticleSystem = parser.ParseFXParticleSystemTemplateReference()
            };
        }

        public string BoneName { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> ParticleSystem { get; private set; }
    }
}
