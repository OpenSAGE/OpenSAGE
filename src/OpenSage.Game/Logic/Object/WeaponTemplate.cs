using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponTemplate : BaseAsset
    {
        internal static WeaponTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("Weapon", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<WeaponTemplate> FieldParseTable = new IniParseTable<WeaponTemplate>
        {
            // Legacy
            { "PrimaryDamage", (parser, x) => x.SetDamageNuggetDamage(0, parser.ParseFloat()) },
            { "PrimaryDamageRadius", (parser, x) => x.SetDamageNuggetRadius(0, parser.ParseFloat()) },
            { "SecondaryDamage", (parser, x) => x.SetDamageNuggetDamage(1, parser.ParseFloat()) },
            { "SecondaryDamageRadius", (parser, x) => x.SetDamageNuggetRadius(1, parser.ParseFloat()) },
            {
                "DamageType",
                (parser, x) =>
                {
                    var damageType = parser.ParseEnum<DamageType>();
                    foreach (var damageNugget in x.GetDamageNuggets())
                    {
                        damageNugget.DamageType = damageType;
                    }
                }
            },
            {
                "DamageStatusType",
                (parser, x) =>
                {
                    var damageStatusType = parser.ParseEnum<DamageStatusType>();
                    foreach (var damageNugget in x.GetDamageNuggets())
                    {
                        damageNugget.DamageStatusType = damageStatusType;
                    }
                }
            },
            {
                "DeathType",
                (parser, x) =>
                {
                    var deathType = parser.ParseEnum<DeathType>();
                    foreach (var damageNugget in x.GetDamageNuggets())
                    {
                        damageNugget.DeathType = deathType;
                    }
                }
            },
            { "ProjectileObject", (parser, x) => x.Nuggets.Add(new ProjectileNugget { ProjectileTemplateName = parser.ParseAssetReference() }) },
            { "ShockWaveAmount", (parser, x) => x.EnsureMetaImpactNugget().ShockWaveAmount = parser.ParseFloat() },
            { "ShockWaveRadius", (parser, x) => x.EnsureMetaImpactNugget().ShockWaveRadius = parser.ParseFloat() },
            { "ShockWaveTaperOff", (parser, x) => x.EnsureMetaImpactNugget().ShockWaveTaperOff = parser.ParseFloat() },

            { "ScatterRadius", (parser, x) => x.ScatterRadius = parser.ParseFloat() },
            { "ScatterRadiusVsInfantry", (parser, x) => x.ScatterRadiusVsInfantry = parser.ParseFloat() },
            { "ScatterTargetScalar", (parser, x) => x.ScatterTargetScalar = parser.ParseFloat() },
            { "ScatterTarget", (parser, x) => x.ScatterTargets.Add(parser.ParseVector2()) },
            { "LeechRangeWeapon", (parser, x) => x.LeechRangeWeapon = parser.ParseBoolean() },
            { "AttackRange", (parser, x) => x.AttackRange = parser.ParseFloat() },
            { "MinimumAttackRange", (parser, x) => x.MinimumAttackRange = parser.ParseFloat() },
            { "MinTargetPitch", (parser, x) => x.MinTargetPitch = parser.ParseInteger() },
            { "MaxTargetPitch", (parser, x) => x.MaxTargetPitch = parser.ParseInteger() },
            { "WeaponSpeed", (parser, x) => x.WeaponSpeed = parser.ParseFloat() },
            { "MinWeaponSpeed", (parser, x) => x.MinWeaponSpeed = parser.ParseFloat() },
            { "MaxWeaponSpeed", (parser, x) => x.MaxWeaponSpeed = parser.ParseInteger() },
            { "ScaleWeaponSpeed", (parser, x) => x.ScaleWeaponSpeed = parser.ParseBoolean() },
            { "WeaponRecoil", (parser, x) => x.WeaponRecoil = parser.ParseInteger() },
            { "FireFX", (parser, x) => x.FireFX = parser.ParseAssetReference() },
            { "PlayFXWhenStealthed", (parser, x) => x.PlayFXWhenStealthed = parser.ParseBoolean() },
            { "FireOCL", (parser, x) => x.FireOCL = parser.ParseAssetReference() },
            { "VeterancyFireFX", (parser, x) => x.VeterancyFireFX = ParseVeterancyAssetReference(parser) },

            { "ProjectileStreamName", (parser, x) => x.ProjectileStreamName = parser.ParseAssetReference() },
            { "ProjectileDetonationFX", (parser, x) => x.ProjectileDetonationFX = parser.ParseAssetReference() },
            { "ProjectileDetonationOCL", (parser, x) => x.ProjectileDetonationOCL = parser.ParseAssetReference() },
            { "ProjectileExhaust", (parser, x) => x.ProjectileExhaust = parser.ParseAssetReference() },
            { "VeterancyProjectileExhaust", (parser, x) => x.VeterancyProjectileExhaust = ParseVeterancyAssetReference(parser) },
            { "ProjectileFilterInContainer", (parser, x) => x.ProjectileFilterInContainer = ObjectFilter.Parse(parser) },
            { "ProjectileSelf", (parser, x) => x.ProjectileSelf = parser.ParseBoolean() },
            { "ProjectileCollidesWith", (parser, x) => x.ProjectileCollidesWith = parser.ParseEnumFlags<WeaponCollideTypes>() },

            { "FireSound", (parser, x) => x.FireSound = parser.ParseAssetReference() },
            { "FireSoundLoopTime", (parser, x) => x.FireSoundLoopTime = parser.ParseInteger() },
            { "SuspendFXDelay", (parser, x) => x.SuspendFXDelay = parser.ParseInteger() },
            { "RadiusDamageAffects", (parser, x) => x.RadiusDamageAffects = ObjectFilter.Parse(parser) },
            { "DelayBetweenShots", (parser, x) => x.DelayBetweenShots = RangeDuration.Parse(parser) },
            { "ShotsPerBarrel", (parser, x) => x.ShotsPerBarrel = parser.ParseInteger() },
            { "ClipSize", (parser, x) => x.ClipSize = parser.ParseInteger() },
            { "ClipReloadTime", (parser, x) => x.ClipReloadTime = RangeDuration.Parse(parser) },
            { "AutoReloadWhenIdle", (parser, x) => x.AutoReloadWhenIdle = parser.ParseInteger() },
            { "AutoReloadsClip", (parser, x) => x.AutoReloadsClip = parser.ParseEnum<WeaponReloadType>() },
            { "ContinuousFireOne", (parser, x) => x.ContinuousFireOne = parser.ParseInteger() },
            { "ContinuousFireTwo", (parser, x) => x.ContinuousFireTwo = parser.ParseInteger() },
            { "ContinuousFireCoast", (parser, x) => x.ContinuousFireCoast = parser.ParseInteger() },
            { "PreAttackDelay", (parser, x) => x.PreAttackDelay = parser.ParseInteger() },
            { "PreAttackType", (parser, x) => x.PreAttackType = parser.ParseEnum<WeaponPrefireType>() },
            { "ContinueAttackRange", (parser, x) => x.ContinueAttackRange = parser.ParseInteger() },
            { "AcceptableAimDelta", (parser, x) => x.AcceptableAimDelta = parser.ParseFloat() },
            { "HitPercentage", (parser, x) => x.HitPercentage = parser.ParsePercentage() },
            { "PreAttackRandomAmount", (parser, x) => x.PreAttackRandomAmount = parser.ParseInteger() },
            { "IsAimingWeapon", (parser, x) => x.IsAimingWeapon = parser.ParseBoolean() },
            { "FXTrigger", (parser, x) => x.FxTrigger = parser.ParseEnumFlags<ObjectKinds>() },
            { "HitStoredTarget", (parser, x) => x.HitStoredTarget = parser.ParseBoolean() },
            { "PreferredTargetBone", (parser, x) => x.PreferredTargetBone = parser.ParseAssetReference() },
            { "MeleeWeapon", (parser, x) => x.MeleeWeapon = parser.ParseBoolean() },
            { "IdleAfterFiringDelay", (parser, x) => x.IdleAfterFiringDelay = parser.ParseInteger() },
            { "HitPassengerPercentage", (parser, x) => x.HitPassengerPercentage = parser.ParsePercentage() },
            { "CanBeDodged", (parser, x) => x.CanBeDodged = parser.ParseBoolean() },
            { "OverrideVoiceAttackSound", (parser, x) => x.OverrideVoiceAttackSound = parser.ParseAssetReference() },
            { "NoVictimNeeded", (parser, x) => x.NoVictimNeeded = parser.ParseBoolean() },
            { "CanFireWhileMoving", (parser, x) => x.CanFireWhileMoving = parser.ParseBoolean() },
            { "RequireFollowThru", (parser, x) => x.RequireFollowThru = parser.ParseBoolean() },
            { "ScatterIndependently", (parser, x) => x.ScatterIndependently = parser.ParseBoolean() },
            { "PreAttackFX", (parser, x) => x.PreAttackFX = parser.ParseAssetReference() },
            { "AimDirection", (parser, x) => x.AimDirection = parser.ParseFloat() },
            { "HoldAfterFiringDelay", (parser, x) => x.HoldAfterFiringDelay = parser.ParseInteger() },
            { "FinishAttackOnceStarted", (parser, x) => x.FinishAttackOnceStarted = parser.ParseBoolean() },
            { "ShareTimers", (parser, x) => x.ShareTimers = parser.ParseBoolean() },
            { "DisableScatterForTargetsOnWall", (parser, x) => x.DisableScatterForTargetsOnWall = parser.ParseBoolean() },
            { "ShouldPlayUnderAttackEvaEvent", (parser, x) => x.ShouldPlayUnderAttackEvaEvent = parser.ParseBoolean() },
            { "CanSwoop", (parser, x) => x.CanSwoop = parser.ParseBoolean() },
            { "PassengerProportionalAttack", (parser, x) => x.PassengerProportionalAttack = parser.ParseBoolean() },
            { "MaxAttackPassengers", (parser, x) => x.MaxAttackPassengers = parser.ParseInteger() },
            { "ChaseWeapon", (parser, x) => x.ChaseWeapon = parser.ParseBoolean() },
            { "CanFireWhileCharging", (parser, x) => x.CanFireWhileCharging = parser.ParseBoolean() },
            { "IgnoreLinearFirstTarget", (parser, x) => x.IgnoreLinearFirstTarget = parser.ParseBoolean() },
            { "LinearTarget", (parser, x) => x.LinearTargets.Add(LinearTarget.Parse(parser)) },
            { "ForceDisplayPercentReady", (parser, x) => x.ForceDisplayPercentReady = parser.ParseBoolean() },
            { "RotatingTurret", (parser, x) => x.RotatingTurret = parser.ParseBoolean() },
            { "RangeBonusMinHeight", (parser, x) => x.RangeBonusMinHeight = parser.ParseInteger() },
            { "RangeBonus", (parser, x) => x.RangeBonus = parser.ParseInteger() },
            { "RangeBonusPerFoot", (parser, x) => x.RangeBonusPerFoot = parser.ParseInteger() },
            { "FireFlankFX", (parser, x) => x.FireFlankFX = parser.ParseAssetReference() },
            { "InstantLoadClipOnActivate", (parser, x) => x.InstantLoadClipOnActivate = parser.ParseBoolean() },
            { "BombardType", (parser, x) => x.BombardType = parser.ParseBoolean() },
            { "OverrideVoiceEnterStateAttackSound", (parser, x) => x.OverrideVoiceEnterStateAttackSound = parser.ParseAssetReference() },
            { "HoldDuringReload", (parser, x) => x.HoldDuringReload = parser.ParseBoolean() },
            { "LockWhenUsing", (parser, x) => x.LockWhenUsing = parser.ParseBoolean() },
            { "UseInnateAttributes", (parser, x) => x.UseInnateAttributes = parser.ParseBoolean() },
            { "AntiMask", (parser, x) => x.AntiMask = parser.ParseEnumFlags<WeaponAntiFlags>() },
            { "ShowsAmmoPips", (parser, x) => x.ShowsAmmoPips = parser.ParseBoolean() },
            { "LaserName", (parser, x) => x.LaserName = parser.ParseAssetReference() },
            { "LaserBoneName", (parser, x) => x.LaserBoneName = parser.ParseBoneName() },
            { "DamageDealtAtSelfPosition", (parser, x) => x.DamageDealtAtSelfPosition = parser.ParseBoolean() },
            { "RequestAssistRange", (parser, x) => x.RequestAssistRange = parser.ParseInteger() },
            { "AllowAttackGarrisonedBldgs", (parser, x) => x.AllowAttackGarrisonedBldgs = parser.ParseBoolean() },
            { "CapableOfFollowingWaypoints", (parser, x) => x.CapableOfFollowingWaypoints = parser.ParseBoolean() },
            { "WeaponBonus", (parser, x) => x.WeaponBonuses.Parse(parser) },
            { "HistoricBonusTime", (parser, x) => x.HistoricBonusTime = parser.ParseInteger() },
            { "HistoricBonusCount", (parser, x) => x.HistoricBonusCount = parser.ParseInteger() },
            { "HistoricBonusRadius", (parser, x) => x.HistoricBonusRadius = parser.ParseInteger() },
            { "HistoricBonusWeapon", (parser, x) => x.HistoricBonusWeapon = parser.ParseAssetReference() },
            { "MissileCallsOnDie", (parser, x) => x.MissileCallsOnDie = parser.ParseBoolean() },
            { "FiringDuration", (parser, x) => x.FiringDuration = parser.ParseInteger() },

            // Anti flags
            { "AntiSmallMissile", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiSmallMissile, parser.ParseBoolean()) },
            { "AntiProjectile", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiProjectile, parser.ParseBoolean()) },
            { "AntiAirborneVehicle", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiAirborneVehicle, parser.ParseBoolean()) },
            { "AntiAirborneInfantry", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiAirborneInfantry, parser.ParseBoolean()) },
            { "AntiGround", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiGround, parser.ParseBoolean()) },
            { "AntiBallisticMissile", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiBallisticMissile, parser.ParseBoolean()) },
            { "AntiMine", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiMine, parser.ParseBoolean()) },
            { "AntiStructure", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiStructure, parser.ParseBoolean()) },
            { "AntiAirborneMonster", (parser, x) => x.SetAntiMaskFlag(WeaponAntiFlags.AntiAirborneMonster, parser.ParseBoolean()) },

            // Nuggets
            { "ProjectileNugget", (parser, x) => x.Nuggets.Add(ProjectileNugget.Parse(parser)) },
            { "DamageNugget", (parser, x) => x.Nuggets.Add(DamageNugget.Parse(parser)) },
            { "MetaImpactNugget", (parser, x) => x.Nuggets.Add(MetaImpactNugget.Parse(parser)) },
            { "SpecialModelConditionNugget", (parser, x) => x.Nuggets.Add(SpecialModelConditionNugget.Parse(parser)) },
            { "ParalyzeNugget", (parser, x) => x.Nuggets.Add(ParalyzeNugget.Parse(parser)) },
            { "HordeAttackNugget", (parser, x) => x.Nuggets.Add(HordeAttackNugget.Parse(parser)) },
            { "SpawnAndFadeNugget", (parser, x) => x.Nuggets.Add(SpawnAndFadeNugget.Parse(parser)) },
            { "AttributeModifierNugget", (parser, x) => x.Nuggets.Add(AttributeModifierNugget.Parse(parser)) },
            { "DamageFieldNugget", (parser, x) => x.Nuggets.Add(DamageFieldNugget.Parse(parser)) },
            { "GrabNugget", (parser, x) => x.Nuggets.Add(GrabNugget.Parse(parser)) },
            { "DOTNugget", (parser, x) => x.Nuggets.Add(DOTNugget.Parse(parser)) },
            { "SlaveAttackNugget", (parser, x) => x.Nuggets.Add(SlaveAttackNugget.Parse(parser)) },
            { "FireLogicNugget", (parser, x) => x.Nuggets.Add(FireLogicNugget.Parse(parser)) },
            { "EmotionWeaponNugget", (parser, x) => x.Nuggets.Add(EmotionWeaponNugget.Parse(parser)) },
            { "OpenGateNugget", (parser, x) => x.Nuggets.Add(OpenGateNugget.Parse(parser)) },
            { "WeaponOCLNugget", (parser, x) => x.Nuggets.Add(WeaponOCLNugget.Parse(parser)) },
            { "LuaEventNugget", (parser, x) => x.Nuggets.Add(LuaEventNugget.Parse(parser)) },
            { "DamageContainedNugget", (parser, x) => x.Nuggets.Add(DamageContainedNugget.Parse(parser)) },
            { "StealMoneyNugget", (parser, x) => x.Nuggets.Add(StealMoneyNugget.Parse(parser)) },
            { "ClearNuggets", (parser, x) => x.Nuggets.Clear() },
        };

        private static string ParseVeterancyAssetReference(IniParser parser)
        {
            var tokenPosition = parser.CurrentPosition;
            var identifier = parser.ParseIdentifier();
            if (identifier != "HEROIC")
            {
                throw new IniParseException($"Unexpected identifier: {identifier}", tokenPosition);
            }
            return parser.ParseAssetReference();
        }

        private IEnumerable<DamageNugget> GetDamageNuggets()
        {
            return Nuggets.OfType<DamageNugget>();
        }

        private DamageNugget EnsureDammageNugget(int index)
        {
            var damageNuggets = GetDamageNuggets().ToList();
            if (damageNuggets.Count < index + 1)
            {
                damageNuggets.Add(new DamageNugget());
            }
            return damageNuggets[index];
        }

        private void SetDamageNuggetDamage(int index, float value)
        {
            var damageNugget = EnsureDammageNugget(index);
            damageNugget.Damage = value;
        }

        private void SetDamageNuggetRadius(int index, float value)
        {
            var damageNugget = EnsureDammageNugget(index);
            damageNugget.Radius = value;
        }

        private MetaImpactNugget EnsureMetaImpactNugget()
        {
            var metaImpactNugget = Nuggets.OfType<MetaImpactNugget>().FirstOrDefault();
            if (metaImpactNugget == null)
            {
                metaImpactNugget = new MetaImpactNugget();
            }
            return metaImpactNugget;
        }

        public float ScatterRadius { get; private set; }
        public float ScatterRadiusVsInfantry { get; private set; }
        public float ScatterTargetScalar { get; private set; }
        public List<Vector2> ScatterTargets { get; } = new List<Vector2>();
        public bool LeechRangeWeapon { get; private set; }
        public float AttackRange { get; private set; }
        public float MinimumAttackRange { get; private set; }
        public int MinTargetPitch { get; private set; }
        public int MaxTargetPitch { get; private set; }

        public float WeaponSpeed { get; private set; }
        public float MinWeaponSpeed { get; private set; }
        public bool ScaleWeaponSpeed { get; private set; }
        public int WeaponRecoil { get; private set; }
        public string FireFX { get; private set; }
        public bool PlayFXWhenStealthed { get; private set; }
        public string FireOCL { get; private set; }
        public string VeterancyFireFX { get; private set; }
        public string ProjectileDetonationFX { get; private set; }
        public string ProjectileDetonationOCL { get; private set; }
        public string ProjectileExhaust { get; private set; }
        public string VeterancyProjectileExhaust { get; private set; }
        public string ProjectileStreamName { get; private set; }
        public string FireSound { get; private set; }
        public int FireSoundLoopTime { get; private set; }
        public int SuspendFXDelay { get; private set; }
        public ObjectFilter RadiusDamageAffects { get; private set; }
        public RangeDuration DelayBetweenShots { get; private set; }
        public int ShotsPerBarrel { get; private set; }
        public int ClipSize { get; private set; }
        public RangeDuration ClipReloadTime { get; private set; }
        public int AutoReloadWhenIdle { get; private set; }
        public WeaponReloadType AutoReloadsClip { get; private set; } = WeaponReloadType.Auto;
        public int ContinuousFireOne { get; private set; }
        public int ContinuousFireTwo { get; private set; }
        public int ContinuousFireCoast { get; private set; }
        public int PreAttackDelay { get; private set; }
        public WeaponPrefireType PreAttackType { get; private set; }
        public int ContinueAttackRange { get; private set; }
        public float AcceptableAimDelta { get; private set; }
        public bool ShowsAmmoPips { get; private set; }
        public string LaserName { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string LaserBoneName { get; private set; }

        public bool DamageDealtAtSelfPosition { get; private set; }
        public int RequestAssistRange { get; private set; }
        public bool AllowAttackGarrisonedBldgs { get; private set; }
        public bool CapableOfFollowingWaypoints { get; private set; }
        public WeaponBonusSet WeaponBonuses { get; } = new WeaponBonusSet();
        public WeaponCollideTypes ProjectileCollidesWith { get; private set; }
        public int HistoricBonusTime { get; private set; }
        public int HistoricBonusCount { get; private set; }
        public int HistoricBonusRadius { get; private set; }
        public string HistoricBonusWeapon { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool MissileCallsOnDie { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FiringDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<WeaponEffectNugget> Nuggets { get; } = new List<WeaponEffectNugget>();

        [AddedIn(SageGame.Bfme2)]
        public int MaxWeaponSpeed { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage HitPercentage { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int PreAttackRandomAmount { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IsAimingWeapon { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectKinds FxTrigger { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HitStoredTarget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string PreferredTargetBone { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool MeleeWeapon { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int IdleAfterFiringDelay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ProjectileSelf { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage HitPassengerPercentage { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CanBeDodged { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string OverrideVoiceAttackSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter ProjectileFilterInContainer { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool NoVictimNeeded { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CanFireWhileMoving { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RequireFollowThru { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ScatterIndependently { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string PreAttackFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float AimDirection { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HoldAfterFiringDelay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool FinishAttackOnceStarted { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShareTimers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool DisableScatterForTargetsOnWall { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShouldPlayUnderAttackEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CanSwoop { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool PassengerProportionalAttack { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MaxAttackPassengers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ChaseWeapon { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CanFireWhileCharging { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IgnoreLinearFirstTarget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<LinearTarget> LinearTargets { get; } = new List<LinearTarget>();

        [AddedIn(SageGame.Bfme)]
        public bool ForceDisplayPercentReady { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RotatingTurret { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int RangeBonusMinHeight { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int RangeBonus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int RangeBonusPerFoot { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string FireFlankFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool InstantLoadClipOnActivate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool BombardType { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string OverrideVoiceEnterStateAttackSound { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool HoldDuringReload { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool LockWhenUsing { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseInnateAttributes { get; private set; }

        public WeaponAntiFlags AntiMask { get; private set; } = WeaponAntiFlags.AntiGround;

        private void SetAntiMaskFlag(WeaponAntiFlags flag, bool set)
        {
            if (set)
            {
                AntiMask |= flag;
            }
            else
            {
                AntiMask &= ~flag;
            }
        }
    }

    public struct RangeDuration
    {
        internal static RangeDuration Parse(IniParser parser)
        {
            var token = parser.GetNextToken(IniParser.SeparatorsColon);
            if (parser.IsFloat(token))
            {
                var value = parser.ScanFloat(token);
                return new RangeDuration
                {
                    Min = value,
                    Max = value
                };
            }

            if (token.Text.ToUpperInvariant() != "MIN")
            {
                throw new IniParseException($"Unexpected range duration: {token.Text}", token.Position);
            }

            var minValue = parser.ScanInteger(parser.GetNextToken());

            return new RangeDuration
            {
                Min = minValue,
                Max = parser.ParseAttributeInteger("Max")
            };
        }

        public float Min { get; private set; }
        public float Max { get; private set; }
    }

    public abstract class WeaponEffectNugget
    {
        private protected static readonly IniParseTable<WeaponEffectNugget> FieldParseTable = new IniParseTable<WeaponEffectNugget>
        {
            { "SpecialObjectFilter", (parser, x) => x.SpecialObjectFilter = ObjectFilter.Parse(parser) },
            { "ForbiddenUpgradeNames", (parser, x) => x.ForbiddenUpgradeNames = parser.ParseAssetReferenceArray() },
            { "RequiredUpgradeNames", (parser, x) => x.RequiredUpgradeNames = parser.ParseAssetReferenceArray() },
        };

        public ObjectFilter SpecialObjectFilter { get; private set; }
        public string[] ForbiddenUpgradeNames { get; private set; }
        public string[] RequiredUpgradeNames { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class ProjectileNugget : WeaponEffectNugget
    {
        internal static ProjectileNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ProjectileNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<ProjectileNugget>
            {
                { "ProjectileTemplateName", (parser, x) => x.ProjectileTemplateName = parser.ParseAssetReference() },
                { "WarheadTemplateName", (parser, x) => x.WarheadTemplateName = parser.ParseAssetReference() },
                
                { "AlwaysAttackHereOffset", (parser, x) => x.AlwaysAttackHereOffset = parser.ParseVector3() },
                { "UseAlwaysAttackOffset", (parser, x) => x.UseAlwaysAttackOffset = parser.ParseBoolean() },
                { "WeaponLaunchBoneSlotOverride", (parser, x) => x.WeaponLaunchBoneSlotOverride = parser.ParseEnum<WeaponSlot>() }
            });

        public string ProjectileTemplateName { get; internal set; }
        public string WarheadTemplateName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Vector3 AlwaysAttackHereOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseAlwaysAttackOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeaponSlot WeaponLaunchBoneSlotOverride { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class DamageNugget : WeaponEffectNugget
    {
        internal static DamageNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private protected static new readonly IniParseTable<DamageNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<DamageNugget>
            {
                { "Damage", (parser, x) => x.Damage = parser.ParseFloat() },
                { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
                { "DelayTime", (parser, x) => x.DelayTime = parser.ParseInteger() },
                { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() },
                { "DamageFXType", (parser, x) => x.DamageFxType = parser.ParseEnum<FxType>() },
                { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() },
                { "DamageSpeed", (parser, x) => x.DamageSpeed = parser.ParseFloat() },
                { "DamageArc", (parser, x) => x.DamageArc = parser.ParseInteger() },
                { "DamageScalar", (parser, x) => x.DamageScalar = DamageScalar.Parse(parser) },
                { "DamageMaxHeight", (parser, x) => x.DamageMaxHeight = parser.ParseInteger() },
                { "AcceptDamageAdd", (parser, x) => x.AcceptDamageAdd = parser.ParseBoolean() },
                { "FlankingBonus", (parser, x) => x.FlankingBonus = parser.ParsePercentage() },
                { "DamageTaperOff", (parser, x) => x.DamageTaperOff = parser.ParseInteger() },
                { "DamageSubType", (parser, x) => x.DamageSubType = parser.ParseEnum<DamageType>() },
                { "DrainLife", (parser, x) => x.DrainLife = parser.ParseBoolean() },
                { "DrainLifeMultiplier", (parser, x) => x.DrainLifeMultiplier = parser.ParseFloat() },
                { "CylinderAOE", (parser, x) => x.CylinderAOE = parser.ParseBoolean() },
                { "DamageArcInverted", (parser, x) => x.DamageArcInverted = parser.ParseBoolean() },
                { "ForceKillObjectFilter", (parser, x) => x.ForceKillObjectFilter = ObjectFilter.Parse(parser) },
                { "DamageMaxHeightAboveTerrain", (parser, x) => x.DamageMaxHeightAboveTerrain = parser.ParseInteger() },
                { "MinRadius", (parser, x) => x.MinRadius = parser.ParseInteger() },
            });

        public float Damage { get; internal set; }
        public float Radius { get; internal set; }
        public int DelayTime { get; private set; }
        public DamageType DamageType { get; internal set; }

        /// <summary>
        /// Used when <see cref="DamageType" is <see cref="DamageType.Status"/> />.
        /// </summary>
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public DamageStatusType DamageStatusType { get; internal set; }

        public FxType DamageFxType { get; private set; }
        public DeathType DeathType { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public float DamageSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DamageArc { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public DamageScalar DamageScalar { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DamageMaxHeight { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AcceptDamageAdd { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage FlankingBonus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int DamageTaperOff { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public DamageType DamageSubType { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool DrainLife { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float DrainLifeMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool CylinderAOE { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool DamageArcInverted { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter ForceKillObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int DamageMaxHeightAboveTerrain { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MinRadius { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class MetaImpactNugget : WeaponEffectNugget
    {
        internal static MetaImpactNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<MetaImpactNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<MetaImpactNugget>
            {
                { "HeroResist", (parser, x) => x.HeroResist = parser.ParseFloat() },
                { "ShockWaveAmount", (parser, x) => x.ShockWaveAmount = parser.ParseFloat() },
                { "ShockWaveRadius", (parser, x) => x.ShockWaveRadius = parser.ParseFloat() },
                { "ShockWaveTaperOff", (parser, x) => x.ShockWaveTaperOff = parser.ParseFloat() },
                { "ShockWaveArc", (parser, x) => x.ShockWaveArc = parser.ParseFloat() },
                { "ShockWaveZMult", (parser, x) => x.ShockWaveZMult = parser.ParseFloat() },
                { "ShockWaveSpeed", (parser, x) => x.ShockWaveSpeed = parser.ParseFloat() },
                { "InvertShockWave", (parser, x) => x.InvertShockWave = parser.ParseBoolean() },
                { "DelayTime", (parser, x) => x.DelayTime = parser.ParseInteger() },
                { "FlipDirection", (parser, x) => x.FlipDirection = parser.ParseBoolean() },
                { "OnlyWhenJustDied", (parser, x) => x.OnlyWhenJustDied = parser.ParseBoolean() },
                { "KillObjectFilter", (parser, x) => x.KillObjectFilter = ObjectFilter.Parse(parser) },
                { "ShockWaveClearRadius", (parser, x) => x.ShockWaveClearRadius = parser.ParseBoolean() },
                { "ShockWaveClearMult", (parser, x) => x.ShockWaveClearMult = parser.ParseFloat() },
                { "ShockWaveClearFlingHeight", (parser, x) => x.ShockWaveClearFlingHeight = parser.ParseInteger() },
                { "ShockWaveArcInverted", (parser, x) => x.ShockWaveArcInverted = parser.ParseBoolean() },
                { "CyclonicFactor", (parser, x) => x.CyclonicFactor = parser.ParseFloat() },
                { "AffectHordes", (parser, x) => x.AffectHordes = parser.ParseBoolean() }
            });

        public float HeroResist { get; private set; }
        public float ShockWaveAmount { get; internal set; }
        public float ShockWaveRadius { get; internal set; }
        public float ShockWaveTaperOff { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveArc { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveZMult { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool InvertShockWave { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DelayTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool FlipDirection { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool OnlyWhenJustDied { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter KillObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShockWaveClearRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ShockWaveClearMult { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int ShockWaveClearFlingHeight { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShockWaveArcInverted { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float CyclonicFactor { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AffectHordes { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class SpecialModelConditionNugget : WeaponEffectNugget
    {
        internal static SpecialModelConditionNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpecialModelConditionNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<SpecialModelConditionNugget>
            {
                { "ModelConditionNames", (parser, x) => x.ModelConditionNames = parser.ParseAssetReferenceArray() },
                { "ModelConditionDuration", (parser, x) => x.ModelConditionDuration = parser.ParseInteger() },
            });

        public string[] ModelConditionNames { get; private set; }
        public int ModelConditionDuration { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class ParalyzeNugget : WeaponEffectNugget
    {
        internal static ParalyzeNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ParalyzeNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<ParalyzeNugget>
            {
                { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
                { "Duration", (parser, x) => x.Duration = parser.ParseInteger() },
                { "ParalyzeFX", (parser, x) => x.ParalyzeFX = parser.ParseAssetReference() },
                { "FreezeAnimation", (parser, x) => x.FreezeAnimation = parser.ParseAssetReference() },
                { "AffectHordeMembers", (parser, x) => x.AffectHordeMembers = parser.ParseBoolean() },
            });

        public float Radius { get; private set; }
        public int Duration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ParalyzeFX { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string FreezeAnimation { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AffectHordeMembers { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class HordeAttackNugget : WeaponEffectNugget
    {
        internal static HordeAttackNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HordeAttackNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<HordeAttackNugget>
            {
                { "LockWeaponSlot", (parser, x) => x.LockWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            });

        [AddedIn(SageGame.Bfme2)]
        public WeaponSlot LockWeaponSlot { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class SpawnAndFadeNugget : WeaponEffectNugget
    {
        internal static SpawnAndFadeNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpawnAndFadeNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<SpawnAndFadeNugget>
            {
                { "ObjectTargetFilter", (parser, x) => x.ObjectTargetFilter = ObjectFilter.Parse(parser) },
                { "SpawnedObjectName", (parser, x) => x.SpawnedObjectName = parser.ParseAssetReference() },
                { "SpawnOffset", (parser, x) => x.SpawnOffset = parser.ParseVector3() }
            });

        public ObjectFilter ObjectTargetFilter {get; private set; } 
        public string SpawnedObjectName {get; private set; } 
        public Vector3 SpawnOffset {get; private set; } 
    }

    [AddedIn(SageGame.Bfme)]
    public class AttributeModifierNugget : WeaponEffectNugget
    {
        internal static AttributeModifierNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AttributeModifierNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<AttributeModifierNugget>
            {
                { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() },
                { "DamageFXType", (parser, x) => x.DamageFxType = parser.ParseEnum<FxType>() },
                { "AntiCategories", (parser, x) => x.AntiCategories = parser.ParseEnumBitArray<ModifierCategory>() },
                { "Radius", (parser, x) => x.Radius = parser.ParseLong() },
                { "AffectHordeMembers", (parser, x) => x.AffectHordeMembers = parser.ParseBoolean() },
            });

        public string AttributeModifier { get; private set; }
        public FxType DamageFxType { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModifierCategory> AntiCategories { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public long Radius { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AffectHordeMembers { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class DamageFieldNugget : WeaponEffectNugget
    {
        internal static DamageFieldNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DamageFieldNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<DamageFieldNugget>
            {
                { "WeaponTemplateName", (parser, x) => x.WeaponTemplateName = parser.ParseAssetReference() },
                { "Duration", (parser, x) => x.Duration = parser.ParseInteger() }
            });

        public string WeaponTemplateName { get; private set; }
        public int Duration { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class GrabNugget : WeaponEffectNugget
    {
        internal static GrabNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GrabNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<GrabNugget>
            {
                { "ContainTargetOnEffect", (parser, x) => x.ContainTargetOnEffect = parser.ParseBoolean() },
                { "ImpactTargetOnEffect", (parser, x) => x.ImpactTargetOnEffect = parser.ParseBoolean() },
                { "ShockWaveAmount", (parser, x) => x.ShockWaveAmount = parser.ParseFloat() },
                { "ShockWaveRadius", (parser, x) => x.ShockWaveRadius = parser.ParseFloat() },
                { "ShockWaveTaperOff", (parser, x) => x.ShockWaveTaperOff = parser.ParseFloat() },
                { "ShockWaveZMult", (parser, x) => x.ShockWaveZMult = parser.ParseFloat() },
                { "RemoveTargetFromOtherContain", (parser, x) => x.RemoveTargetFromOtherContain = parser.ParseBoolean() }
            });

        public bool ContainTargetOnEffect { get; private set; }
        public bool ImpactTargetOnEffect { get; private set; }
        public float ShockWaveAmount { get; private set; }
        public float ShockWaveRadius { get; private set; }
        public float ShockWaveTaperOff { get; private set; }
        public float ShockWaveZMult { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool RemoveTargetFromOtherContain { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class DOTNugget : DamageNugget
    {
        internal static new DOTNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DOTNugget> FieldParseTable = DamageNugget.FieldParseTable
            .Concat(new IniParseTable<DOTNugget>
            {
                { "DamageInterval", (parser, x) => x.DamageInterval = parser.ParseInteger() },
                { "DamageDuration", (parser, x) => x.DamageDuration = parser.ParseInteger() },
            });

        public int DamageInterval { get; private set; }
        public int DamageDuration { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class SlaveAttackNugget : WeaponEffectNugget
    {
        internal static SlaveAttackNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SlaveAttackNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<SlaveAttackNugget>
            {
            });
    }

    [AddedIn(SageGame.Bfme2)]
    public class FireLogicNugget : DamageNugget
    {
        internal static new FireLogicNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FireLogicNugget> FieldParseTable = DamageNugget.FieldParseTable
            .Concat(new IniParseTable<FireLogicNugget>
            {
                { "LogicType", (parser, x) => x.LogicType = parser.ParseEnum<FireLogicType>() },
                { "MinMaxBurnRate", (parser, x) => x.MinMaxBurnRate = parser.ParseInteger() },
                { "MinDecay", (parser, x) => x.MinDecay = parser.ParseInteger() },
                { "MaxResistance", (parser, x) => x.MaxResistance = parser.ParseInteger() },
            });

        public FireLogicType LogicType { get; private set; }
        public int MinMaxBurnRate { get; private set; }
        public int MinDecay { get; private set; }
        public int MaxResistance { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class EmotionWeaponNugget : WeaponEffectNugget
    {
        internal static EmotionWeaponNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<EmotionWeaponNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<EmotionWeaponNugget>
            {
                { "EmotionType", (parser, x) => x.EmotionType = parser.ParseEnum<EmotionType>() },
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "Duration", (parser, x) => x.Duration = parser.ParseInteger() },
            });

        public EmotionType EmotionType { get; private set; }
        public int Radius { get; private set; }
        public int Duration { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class OpenGateNugget : WeaponEffectNugget
    {
        internal static OpenGateNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<OpenGateNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<OpenGateNugget>
            {
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            });

        public int Radius { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class WeaponOCLNugget : WeaponEffectNugget
    {
        internal static WeaponOCLNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WeaponOCLNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<WeaponOCLNugget>
            {
                { "WeaponOCLName", (parser, x) => x.WeaponOCLName = parser.ParseAssetReference() },
            });

        public string WeaponOCLName { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class LuaEventNugget : WeaponEffectNugget
    {
        internal static LuaEventNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<LuaEventNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<LuaEventNugget>
            {
                { "LuaEvent", (parser, x) => x.LuaEvent = parser.ParseAssetReference() },
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "SendToEnemies", (parser, x) => x.SendToEnemies = parser.ParseBoolean() },
                { "SendToAllies", (parser, x) => x.SendToAllies = parser.ParseBoolean() },
                { "SendToNeutral", (parser, x) => x.SendToNeutral = parser.ParseBoolean() },
            });

        public string LuaEvent { get; private set; }
        public int Radius { get; private set; }
        public bool SendToEnemies { get; private set; }
        public bool SendToAllies { get; private set; }
        public bool SendToNeutral { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class DamageContainedNugget : DamageNugget
    {
        internal static new DamageContainedNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DamageContainedNugget> FieldParseTable = DamageNugget.FieldParseTable
            .Concat(new IniParseTable<DamageContainedNugget>
            {
                { "KillCount", (parser, x) => x.KillCount = parser.ParseInteger() },
                { "KillKindof", (parser, x) => x.KillKindof = parser.ParseEnum<ObjectKinds>() },
                { "KillKindofNot", (parser, x) => x.KillKindofNot = parser.ParseEnum<ObjectKinds>() },
            });

        public int KillCount { get; private set; }
        public ObjectKinds KillKindof { get; private set; }
        public ObjectKinds KillKindofNot { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class StealMoneyNugget : WeaponEffectNugget
    {
        internal static StealMoneyNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StealMoneyNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<StealMoneyNugget>
            {
                { "AmountStolenPerAttack", (parser, x) => x.AmountStolenPerAttack = parser.ParseInteger() },
            });

        public int AmountStolenPerAttack { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class DamageScalar
    {
        internal static DamageScalar Parse(IniParser parser)
        {
            var result = new DamageScalar
            {
                Scalar = parser.ParsePercentage()
            };
            var token = parser.PeekNextTokenOptional();
            if (token.HasValue)
            {
                result.Targets = ObjectFilter.Parse(parser);
            }
            return result;
        }

        public Percentage Scalar { get; private set; }
        public ObjectFilter Targets { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class LinearTarget
    {
        internal static LinearTarget Parse(IniParser parser)
        {
            var offset = parser.ParseVector2();
            var t = parser.ParseAttributeFloat("T");
            return new LinearTarget { Offset = offset, T = t};
        }

        public Vector2 Offset { get; private set; }
        public float T { get; private set; } //not sure what this is
    }


    public enum WeaponReloadType
    {
        [IniEnum("No")]
        None,

        [IniEnum("Yes")]
        Auto,

        [IniEnum("RETURN_TO_BASE")]
        ReturnToBase
    }

    public enum WeaponPrefireType
    {
        [IniEnum("PER_ATTACK")]
        PerAttack,

        [IniEnum("PER_CLIP")]
        PerClip,

        [IniEnum("PER_SHOT")]
        PerShot,

        [IniEnum("PER_POSITION"), AddedIn(SageGame.Bfme)]
        PerPosition
    }

    public enum DeathType
    {
        [IniEnum("NORMAL")]
        Normal,

        [IniEnum("EXPLODED")]
        Exploded,

        [IniEnum("LASERED")]
        Lasered,

        [IniEnum("BURNED")]
        Burned,

        [IniEnum("SUICIDED")]
        Suicided,

        [IniEnum("POISONED")]
        Poisoned,

        [IniEnum("POISONED_BETA")]
        PoisonedBeta,

        [IniEnum("POISONED_GAMMA"), AddedIn(SageGame.CncGeneralsZeroHour)]
        PoisonedGamma,

        [IniEnum("CRUSHED")]
        Crushed,

        [IniEnum("TOPPLED")]
        Toppled,

        [IniEnum("SPLATTED")]
        Splatted,

        [IniEnum("FLOODED")]
        Flooded,

        [IniEnum("DETONATED")]
        Detonated,

        [IniEnum("EXTRA_4"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Extra4,

        [IniEnum("FADED"), AddedIn(SageGame.Bfme)]
        Faded,

        [IniEnum("KNOCKBACK"), AddedIn(SageGame.Bfme)]
        Knockback,

        [IniEnum("EXTRA_2"), AddedIn(SageGame.Bfme)]
        Extra2,

        [IniEnum("SUPERNATURAL"), AddedIn(SageGame.Bfme)]
        Supernatural,

        [IniEnum("SLAUGHTERED"), AddedIn(SageGame.Bfme2)]
        Slaughtered,

        [IniEnum("EXTRA_1"), AddedIn(SageGame.Bfme)]
        Extra1,

        [IniEnum("EXTRA_3"), AddedIn(SageGame.Bfme)]
        Extra3,

        [IniEnum("EXTRA_5"), AddedIn(SageGame.Bfme2Rotwk)]
        Extra5,

        [IniEnum("EXTRA_6"), AddedIn(SageGame.Bfme2Rotwk)]
        Extra6,

        [IniEnum("EXTRA_7"), AddedIn(SageGame.Bfme2Rotwk)]
        Extra7,

        [IniEnum("EXTRA_8"), AddedIn(SageGame.Bfme2Rotwk)]
        Extra8,
    }

    [Flags]
    public enum WeaponCollideTypes
    {
        None = 0,

        [IniEnum("STRUCTURES")]
        Structures = 1 << 0,
        
        [IniEnum("WALLS")]
        Walls = 1 << 1,

        [IniEnum("ENEMIES")]
        Enemies = 1 << 2,

        [IniEnum("SHRUBBERY")]
        Shrubbery = 1 << 3,

        [IniEnum("ALLIES")]
        Allies = 1 << 4,

        [IniEnum("NEUTRAL")]
        Neutral = 1 << 5,

        [IniEnum("MONSTERS")]
        Monsters = 1 << 6,
    }

    public enum DamageStatusType
    {
        [IniEnum("FAERIE_FIRE")]
        FaerieFire,
    }

    [AddedIn(SageGame.Bfme2)]
    public enum FireLogicType
    {
        [IniEnum("INCREASE_BURN_RATE")]
        IncreaseBurnRate,

        [IniEnum("INCREASE_FUEL")]
        IncreaseFuel,

        [IniEnum("INCREASE_FUEL_ON_EXISTING_FIRE")]
        IncreaseFuelOnExistingFire,

        [IniEnum("INCREASE_BURN_RATE_ON_EXISTING_FIRE")]
        IncreaseBurnRateOnExistingFire,

        [IniEnum("DECREASE_BURN_RATE")]
        DecreaseBurnRate,
    }

    [Flags]
    public enum WeaponAntiFlags
    {
        None                 = 0,
        AntiAirborneVehicle  = 1 << 0,
        AntiGround           = 1 << 1,
        AntiProjectile       = 1 << 2,
        AntiSmallMissile     = 1 << 3,
        AntiMine             = 1 << 4,
        AntiAirborneInfantry = 1 << 5,
        AntiBallisticMissile = 1 << 6,
        AntiParachute        = 1 << 7,
        AntiStructure        = 1 << 8,
        AntiAirborneMonster  = 1 << 9,
    }
}
