using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class ModelConditionState : IConditionState
    {
        internal static void Parse(IniParser parser, ModelConditionState result)
        {
            parser.ParseBlock(FieldParseTable, result);
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

            { "Turret", (parser, x) => x.Turret = parser.ParseAssetReference() },
            { "TurretArtAngle", (parser, x) => x.TurretArtAngle = parser.ParseInteger() },
            { "TurretPitch", (parser, x) => x.TurretPitch = parser.ParseAssetReference() },
            { "AltTurret", (parser, x) => x.AltTurret = parser.ParseAssetReference() },
            { "AltTurretPitch", (parser, x) => x.AltTurretPitch = parser.ParseAssetReference() },

            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },

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

        internal ModelConditionState Clone()
        {
            var result = (ModelConditionState) MemberwiseClone();

            result.ConditionFlags.AddRange(result.ConditionFlags);
            result.WeaponRecoilBones.AddRange(result.WeaponRecoilBones);
            result.WeaponFireFXBones.AddRange(result.WeaponFireFXBones);
            result.WeaponMuzzleFlashes.AddRange(result.WeaponMuzzleFlashes);
            result.WeaponLaunchBones.AddRange(result.WeaponLaunchBones);
            result.WeaponHideShowBones.AddRange(result.WeaponHideShowBones);
            result.ParticleSysBones.AddRange(result.ParticleSysBones);
            result.FXEvents = new List<FXEvent>(result.FXEvents);

            return result;
        }

        public List<BitArray<ModelConditionFlag>> ConditionFlags { get; } = new();

        public LazyAssetReference<Model> Model;

        [AddedIn(SageGame.Bfme)]
        public string Skeleton { get; private set; }

        // Weapon bone settings
        public readonly List<BoneAttachPoint> WeaponRecoilBones = new List<BoneAttachPoint>();
        public readonly List<BoneAttachPoint> WeaponFireFXBones = new List<BoneAttachPoint>();
        public readonly List<BoneAttachPoint> WeaponMuzzleFlashes = new List<BoneAttachPoint>();
        public readonly List<BoneAttachPoint> WeaponLaunchBones = new List<BoneAttachPoint>();
        public readonly List<BoneAttachPoint> WeaponHideShowBones = new List<BoneAttachPoint>();

        // Turret settings
        public string Turret;
        public int TurretArtAngle;
        public string TurretPitch;
        public string AltTurret;
        public string AltTurretPitch;

        // Misc settings
        public readonly List<ParticleSysBone> ParticleSysBones = new List<ParticleSysBone>();

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
    }

    /// <summary>
    /// For Generals and Zero Hour, we first parse ConditionState and TransitionState into one of these objects.
    /// Then we convert it to the BFME-style ModelConditionState, AnimationState, and TransitionState objects.
    /// </summary>
    internal class ModelConditionStateGenerals
    {
        internal static void Parse(IniParser parser, ModelConditionStateGenerals result)
        {
            parser.ParseBlock(FieldParseTable, result);
        }

        internal static readonly IniParseTable<ModelConditionStateGenerals> FieldParseTable = new IniParseTable<ModelConditionStateGenerals>
        {
            { "Model", (parser, x) => { x.Model = parser.ParseModelReference(); x.OnNonAnimationPropertySet(); } },

            { "WeaponRecoilBone", (parser, x) => { x.WeaponRecoilBones.Add(BoneAttachPoint.Parse(parser)); x.OnNonAnimationPropertySet(); } },
            { "WeaponFireFXBone", (parser, x) => { x.WeaponFireFXBones.Add(BoneAttachPoint.Parse(parser)); x.OnNonAnimationPropertySet(); } },
            { "WeaponMuzzleFlash", (parser, x) => { x.WeaponMuzzleFlashes.Add(BoneAttachPoint.Parse(parser)); x.OnNonAnimationPropertySet(); } },
            { "WeaponLaunchBone", (parser, x) => { x.WeaponLaunchBones.Add(BoneAttachPoint.Parse(parser)); x.OnNonAnimationPropertySet(); } },
            { "WeaponHideShowBone", (parser, x) => { x.WeaponHideShowBones.Add(BoneAttachPoint.Parse(parser)); x.OnNonAnimationPropertySet(); } },

            { "Turret", (parser, x) => { x.Turret = parser.ParseAssetReference(); x.OnNonAnimationPropertySet(); } },
            { "TurretArtAngle", (parser, x) => { x.TurretArtAngle = parser.ParseInteger(); x.OnNonAnimationPropertySet(); } },
            { "TurretPitch", (parser, x) => { x.TurretPitch = parser.ParseAssetReference(); x.OnNonAnimationPropertySet(); } },
            { "AltTurret", (parser, x) => { x.AltTurret = parser.ParseAssetReference(); x.OnNonAnimationPropertySet(); } },
            { "AltTurretPitch", (parser, x) => { x.AltTurretPitch = parser.ParseAssetReference(); x.OnNonAnimationPropertySet(); } },

            { "HideSubObject", (parser, x) => { x.HideSubObject = parser.ParseAssetReferenceArray(); x.OnNonAnimationPropertySet(); } },
            { "ShowSubObject", (parser, x) => { x.ShowSubObject = parser.ParseAssetReferenceArray(); x.OnNonAnimationPropertySet(); } },
            { "ParticleSysBone", (parser, x) => { x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)); x.OnNonAnimationPropertySet(); } },

            { "Animation", (parser, x) => { x.ParseAnimation(parser, false); x.OnAnimationPropertySet(); } },
            { "AnimationMode", (parser, x) => { x.AnimationMode = parser.ParseEnum<AnimationMode>(); x.OnAnimationPropertySet(); } },
            { "AnimationSpeedFactorRange", (parser, x) => { x.AnimationSpeedFactorRange = parser.ParseFloatRange(); x.OnAnimationPropertySet(); } },
            { "IdleAnimation", (parser, x) => { x.ParseAnimation(parser, true); x.OnAnimationPropertySet(); } },
            { "Flags", (parser, x) => { x.Flags = parser.ParseEnumFlags<AnimationFlags>(); x.OnAnimationPropertySet(); } },
            { "TransitionKey", (parser, x) => { x.TransitionKey = parser.ParseIdentifier(); x.OnAnimationPropertySet(); } },
            { "WaitForStateToFinishIfPossible", (parser, x) => { x.WaitForStateToFinishIfPossible = parser.ParseIdentifier(); x.OnAnimationPropertySet(); } },
        };

        internal bool NonAnimationPropertySet { get; private set; }
        internal bool AnimationPropertySet { get; private set; }

        private void OnNonAnimationPropertySet() => NonAnimationPropertySet = true;
        private void OnAnimationPropertySet() => AnimationPropertySet = true;

        private void ParseAnimation(IniParser parser, bool isIdle)
        {
            ConditionAnimations.Add(ObjectConditionAnimation.Parse(parser));
        }

        internal ModelConditionStateGenerals Clone()
        {
            var result = (ModelConditionStateGenerals)MemberwiseClone();

            result.NonAnimationPropertySet = false;
            result.AnimationPropertySet = false;

            result.ConditionFlags = new List<BitArray<ModelConditionFlag>>();
            result.ConditionAnimations = new List<ObjectConditionAnimation>();
            result.WeaponRecoilBones = new List<BoneAttachPoint>(result.WeaponRecoilBones);
            result.WeaponFireFXBones = new List<BoneAttachPoint>(result.WeaponFireFXBones);
            result.WeaponMuzzleFlashes = new List<BoneAttachPoint>(result.WeaponMuzzleFlashes);
            result.WeaponLaunchBones = new List<BoneAttachPoint>(result.WeaponLaunchBones);
            result.WeaponHideShowBones = new List<BoneAttachPoint>(result.WeaponHideShowBones);
            result.ParticleSysBones = new List<ParticleSysBone>(result.ParticleSysBones);

            return result;
        }

        public List<BitArray<ModelConditionFlag>> ConditionFlags { get; private set; } = new List<BitArray<ModelConditionFlag>>();

        public LazyAssetReference<Model> Model { get; private set; }

        // Weapon bone settings
        public List<BoneAttachPoint> WeaponRecoilBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponFireFXBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponMuzzleFlashes { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponLaunchBones { get; private set; } = new List<BoneAttachPoint>();
        public List<BoneAttachPoint> WeaponHideShowBones { get; private set; } = new List<BoneAttachPoint>();

        // Model animation settings
        public List<ObjectConditionAnimation> ConditionAnimations { get; private set; } = new List<ObjectConditionAnimation>();
        public AnimationMode AnimationMode { get; private set; }
        public FloatRange AnimationSpeedFactorRange { get; private set; } = new FloatRange(1.0f, 1.0f);
        public AnimationFlags Flags { get; private set; }
        public string TransitionKey { get; private set; }
        public string WaitForStateToFinishIfPossible { get; private set; }

        // Turret settings
        public string Turret { get; private set; }
        public int TurretArtAngle { get; private set; }
        public string TurretPitch { get; private set; }
        public string AltTurret { get; private set; }
        public string AltTurretPitch { get; private set; }

        // Misc settings
        public string[] HideSubObject { get; private set; } = Array.Empty<string>();
        public string[] ShowSubObject { get; private set; } = Array.Empty<string>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();

        public void CopyTo(ModelConditionState other)
        {
            other.ConditionFlags.AddRange(ConditionFlags);

            other.Model = Model;

            other.Turret = Turret;
            other.TurretArtAngle = TurretArtAngle;
            other.TurretPitch = TurretPitch;
            other.AltTurret = AltTurret;
            other.AltTurretPitch = AltTurretPitch;

            other.WeaponRecoilBones.AddRange(WeaponRecoilBones);
            other.WeaponFireFXBones.AddRange(WeaponFireFXBones);
            other.WeaponMuzzleFlashes.AddRange(WeaponMuzzleFlashes);
            other.WeaponLaunchBones.AddRange(WeaponLaunchBones);
            other.WeaponHideShowBones.AddRange(WeaponHideShowBones);
            other.ParticleSysBones.AddRange(ParticleSysBones);
        }

        public void CopyTo(AnimationState other)
        {
            other.ConditionFlags.AddRange(ConditionFlags);

            other.Flags = Flags;

            foreach (var animation in ConditionAnimations)
            {
                other.Animations.Add(new AnimationStateAnimation
                {
                    Animation = animation.Animation,
                    Distance = animation.Distance,
                    Priority = animation.Priority,
                    AnimationMode = AnimationMode,
                    AnimationSpeedFactorRange = AnimationSpeedFactorRange,
                });
            }
        }
    }

    internal sealed class TransitionStateGenerals : ModelConditionStateGenerals
    {
        public string From;
        public string To;
    }

    public sealed class ObjectConditionAnimation
    {
        internal static ObjectConditionAnimation Parse(IniParser parser)
        {
            var result = new ObjectConditionAnimation
            {
                Animation = parser.ParseAnimationReference()
            };

            var distanceToken = parser.GetNextTokenOptional();

            if (distanceToken != null)
            {
                result.Distance = parser.ScanFloat(distanceToken.Value);

                var priorityToken = parser.GetNextTokenOptional();
                if (priorityToken != null)
                {
                    result.Priority = parser.ScanInteger(priorityToken.Value);
                }
            }

            return result;
        }

        public LazyAssetReference<Graphics.Animation.W3DAnimation> Animation { get; private set; }
        public float Distance { get; private set; }
        public int Priority { get; private set; }
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
