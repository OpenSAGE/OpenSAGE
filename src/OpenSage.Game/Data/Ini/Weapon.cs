using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using System.Numerics;

namespace OpenSage.Data.Ini
{
    public sealed class Weapon
    {
        internal static Weapon Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Weapon> FieldParseTable = new IniParseTable<Weapon>
        {
            { "PrimaryDamage", (parser, x) => x.PrimaryDamage = parser.ParseFloat() },
            { "PrimaryDamageRadius", (parser, x) => x.PrimaryDamageRadius = parser.ParseFloat() },
            { "ScatterRadius", (parser, x) => x.ScatterRadius = parser.ParseFloat() },
            { "ScatterRadiusVsInfantry", (parser, x) => x.ScatterRadiusVsInfantry = parser.ParseFloat() },
            { "ScatterTargetScalar", (parser, x) => x.ScatterTargetScalar = parser.ParseFloat() },
            { "ScatterTarget", (parser, x) => x.ScatterTargets.Add(parser.ParseVector2()) },
            { "SecondaryDamage", (parser, x) => x.SecondaryDamage = parser.ParseFloat() },
            { "SecondaryDamageRadius", (parser, x) => x.SecondaryDamageRadius = parser.ParseFloat() },
            { "LeechRangeWeapon", (parser, x) => x.LeechRangeWeapon = parser.ParseBoolean() },
            { "AttackRange", (parser, x) => x.AttackRange = parser.ParseFloat() },
            { "MinimumAttackRange", (parser, x) => x.MinimumAttackRange = parser.ParseFloat() },
            { "MinTargetPitch", (parser, x) => x.MinTargetPitch = parser.ParseInteger() },
            { "MaxTargetPitch", (parser, x) => x.MaxTargetPitch = parser.ParseInteger() },
            { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() },
            { "DamageStatusType", (parser, x) => x.DamageStatusType = parser.ParseEnum<DamageStatusType>() },
            { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() },
            { "WeaponSpeed", (parser, x) => x.WeaponSpeed = parser.ParseFloat() },
            { "MinWeaponSpeed", (parser, x) => x.MinWeaponSpeed = parser.ParseFloat() },
            { "ScaleWeaponSpeed", (parser, x) => x.ScaleWeaponSpeed = parser.ParseBoolean() },
            { "WeaponRecoil", (parser, x) => x.WeaponRecoil = parser.ParseInteger() },
            { "FireFX", (parser, x) => x.FireFX = parser.ParseAssetReference() },
            { "PlayFXWhenStealthed", (parser, x) => x.PlayFXWhenStealthed = parser.ParseBoolean() },
            { "FireOCL", (parser, x) => x.FireOCL = parser.ParseAssetReference() },
            { "VeterancyFireFX", (parser, x) => x.VeterancyFireFX = ParseVeterancyAssetReference(parser) },
            { "ProjectileDetonationFX", (parser, x) => x.ProjectileDetonationFX = parser.ParseAssetReference() },
            { "ProjectileDetonationOCL", (parser, x) => x.ProjectileDetonationOCL = parser.ParseAssetReference() },
            { "ProjectileObject", (parser, x) => x.ProjectileObject = parser.ParseAssetReference() },
            { "ProjectileExhaust", (parser, x) => x.ProjectileExhaust = parser.ParseAssetReference() },
            { "VeterancyProjectileExhaust", (parser, x) => x.VeterancyProjectileExhaust = ParseVeterancyAssetReference(parser) },
            { "ProjectileStreamName", (parser, x) => x.ProjectileStreamName = parser.ParseAssetReference() },
            { "FireSound", (parser, x) => x.FireSound = parser.ParseAssetReference() },
            { "FireSoundLoopTime", (parser, x) => x.FireSoundLoopTime = parser.ParseInteger() },
            { "SuspendFXDelay", (parser, x) => x.SuspendFXDelay = parser.ParseInteger() },
            { "RadiusDamageAffects", (parser, x) => x.RadiusDamageAffects = parser.ParseEnumFlags<WeaponAffectsTypes>() },
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
            { "AntiSmallMissile", (parser, x) => x.AntiSmallMissile = parser.ParseBoolean() },
            { "AntiProjectile", (parser, x) => x.AntiProjectile = parser.ParseBoolean() },
            { "AntiAirborneVehicle", (parser, x) => x.AntiAirborneVehicle = parser.ParseBoolean() },
            { "AntiAirborneInfantry", (parser, x) => x.AntiAirborneInfantry = parser.ParseBoolean() },
            { "AntiGround", (parser, x) => x.AntiGround = parser.ParseBoolean() },
            { "AntiBallisticMissile", (parser, x) => x.AntiBallisticMissile = parser.ParseBoolean() },
            { "AntiMine", (parser, x) => x.AntiMine = parser.ParseBoolean() },
            { "ShowsAmmoPips", (parser, x) => x.ShowsAmmoPips = parser.ParseBoolean() },
            { "LaserName", (parser, x) => x.LaserName = parser.ParseAssetReference() },
            { "LaserBoneName", (parser, x) => x.LaserBoneName = parser.ParseBoneName() },
            { "DamageDealtAtSelfPosition", (parser, x) => x.DamageDealtAtSelfPosition = parser.ParseBoolean() },
            { "RequestAssistRange", (parser, x) => x.RequestAssistRange = parser.ParseInteger() },
            { "AllowAttackGarrisonedBldgs", (parser, x) => x.AllowAttackGarrisonedBldgs = parser.ParseBoolean() },
            { "CapableOfFollowingWaypoints", (parser, x) => x.CapableOfFollowingWaypoints = parser.ParseBoolean() },
            { "WeaponBonus", (parser, x) => x.WeaponBonuses.Parse(parser) },
            { "ProjectileCollidesWith", (parser, x) => x.ProjectileCollidesWith = parser.ParseEnumFlags<WeaponCollideTypes>() },
            { "HistoricBonusTime", (parser, x) => x.HistoricBonusTime = parser.ParseInteger() },
            { "HistoricBonusCount", (parser, x) => x.HistoricBonusCount = parser.ParseInteger() },
            { "HistoricBonusRadius", (parser, x) => x.HistoricBonusRadius = parser.ParseInteger() },
            { "HistoricBonusWeapon", (parser, x) => x.HistoricBonusWeapon = parser.ParseAssetReference() },
            { "ShockWaveAmount", (parser, x) => x.ShockWaveAmount = parser.ParseFloat() },
            { "ShockWaveRadius", (parser, x) => x.ShockWaveRadius = parser.ParseFloat() },
            { "ShockWaveTaperOff", (parser, x) => x.ShockWaveTaperOff = parser.ParseFloat() },
            { "MissileCallsOnDie", (parser, x) => x.MissileCallsOnDie = parser.ParseBoolean() },
            { "FiringDuration", (parser, x) => x.FiringDuration = parser.ParseInteger() },
            { "ProjectileNugget", (parser, x) => x.ProjectileNugget = ProjectileNugget.Parse(parser) },
            { "DamageNugget", (parser, x) => x.DamageNugget = DamageNugget.Parse(parser) },
            { "MetaImpactNugget", (parser, x) => x.MetaImpactNugget = MetaImpactNugget.Parse(parser) },
            { "MaxWeaponSpeed", (parser, x) => x.MaxWeaponSpeed = parser.ParseInteger() },
            { "HitPercentage", (parser, x) => x.HitPercentage = parser.ParsePercentage() },
            { "PreAttackRandomAmount", (parser, x) => x.PreAttackRandomAmount = parser.ParseInteger() },
            { "IsAimingWeapon", (parser, x) => x.IsAimingWeapon = parser.ParseBoolean() },
            { "AntiAirborneMonster", (parser, x) => x.AntiAirborneMonster = parser.ParseBoolean() },
            { "FXTrigger", (parser, x) => x.FxTrigger = parser.ParseEnumFlags<ObjectKinds>() },

            { "ClearNuggets", (parser, x) => x.ClearNuggets() },

            { "SpecialModelConditionNugget", (parser, x) => x.SpecialModelConditionNugget = SpecialModelConditionNugget.Parse(parser) },
            { "ParalyzeNugget", (parser, x) => x.ParalyzeNugget = ParalyzeNugget.Parse(parser) },
            { "HitStoredTarget", (parser, x) => x.HitStoredTarget = parser.ParseBoolean() },
            { "PreferredTargetBone", (parser, x) => x.PreferredTargetBone = parser.ParseAssetReference() },
            { "MeleeWeapon", (parser, x) => x.MeleeWeapon = parser.ParseBoolean() },
            { "IdleAfterFiringDelay", (parser, x) => x.IdleAfterFiringDelay = parser.ParseInteger() },
            { "ProjectileSelf", (parser, x) => x.ProjectileSelf = parser.ParseBoolean() },
            { "HitPassengerPercentage", (parser, x) => x.HitPassengerPercentage = parser.ParsePercentage() },
            { "CanBeDodged", (parser, x) => x.CanBeDodged = parser.ParseBoolean() },
            { "OverrideVoiceAttackSound", (parser, x) => x.OverrideVoiceAttackSound = parser.ParseAssetReference() },
            { "ProjectileFilterInContainer", (parser, x) => x.ProjectileFilterInContainer = ObjectFilter.Parse(parser) },
            { "NoVictimNeeded", (parser, x) => x.NoVictimNeeded = parser.ParseBoolean() },
            { "CanFireWhileMoving", (parser, x) => x.CanFireWhileMoving = parser.ParseBoolean() },
            { "AntiStructure", (parser, x) => x.AntiStructure = parser.ParseBoolean() },
            { "RequireFollowThru", (parser, x) => x.RequireFollowThru = parser.ParseBoolean() },
            { "ScatterIndependently", (parser, x) => x.ScatterIndependently = parser.ParseBoolean() },
            { "PreAttackFX", (parser, x) => x.PreAttackFX = parser.ParseAssetReference() },
            { "AimDirection", (parser, x) => x.AimDirection = parser.ParseFloat() },
            { "HoldAfterFiringDelay", (parser, x) => x.HoldAfterFiringDelay = parser.ParseInteger() },
            { "FinishAttackOnceStarted", (parser, x) => x.FinishAttackOnceStarted = parser.ParseBoolean() },
            { "HordeAttackNugget", (parser, x) => x.HordeAttackNugget = HordeAttackNugget.Parse(parser) },
            { "SpawnAndFadeNugget", (parser, x) => x.SpawnAndFadeNugget = SpawnAndFadeNugget.Parse(parser) },
            { "ShareTimers", (parser, x) => x.ShareTimers = parser.ParseBoolean() },
            { "DisableScatterForTargetsOnWall", (parser, x) => x.DisableScatterForTargetsOnWall = parser.ParseBoolean() },
            { "AttributeModifierNugget", (parser, x) => x.AttributeModifierNugget = AttributeModifierNugget.Parse(parser) },
            { "ShouldPlayUnderAttackEvaEvent", (parser, x) => x.ShouldPlayUnderAttackEvaEvent = parser.ParseBoolean() },
            { "CanSwoop", (parser, x) => x.CanSwoop = parser.ParseBoolean() },
            { "DamageFieldNugget", (parser, x) => x.DamageFieldNugget = DamageFieldNugget.Parse(parser) },
            { "PassengerProportionalAttack", (parser, x) => x.PassengerProportionalAttack = parser.ParseBoolean() },
            { "MaxAttackPassengers", (parser, x) => x.MaxAttackPassengers = parser.ParseInteger() },
            { "ChaseWeapon", (parser, x) => x.ChaseWeapon = parser.ParseBoolean() },
            { "CanFireWhileCharging", (parser, x) => x.CanFireWhileCharging = parser.ParseBoolean() },
            { "IgnoreLinearFirstTarget", (parser, x) => x.IgnoreLinearFirstTarget = parser.ParseBoolean() },
            { "LinearTarget", (parser, x) => x.LinearTargets.Add(LinearTarget.Parse(parser)) },
            { "ForceDisplayPercentReady", (parser, x) => x.ForceDisplayPercentReady = parser.ParseBoolean() },
            { "GrabNugget", (parser, x) => x.GrabNugget = GrabNugget.Parse(parser) },
            { "RotatingTurret", (parser, x) => x.RotatingTurret = parser.ParseBoolean() },
        };

        private void ClearNuggets()
        {
            ProjectileNugget = null;
            DamageNugget = null;
            MetaImpactNugget = null;
            ParalyzeNugget = null;
            SpecialModelConditionNugget = null;
            HordeAttackNugget = null;
            SpawnAndFadeNugget = null;
            AttributeModifierNugget = null;
            DamageFieldNugget = null;
            GrabNugget = null;
        }

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

        public string Name { get; private set; }

        public float PrimaryDamage { get; private set; }
        public float PrimaryDamageRadius { get; private set; }
        public float ScatterRadius { get; private set; }
        public float ScatterRadiusVsInfantry { get; private set; }
        public float ScatterTargetScalar { get; private set; }
        public List<Vector2> ScatterTargets { get; } = new List<Vector2>();
        public float SecondaryDamage { get; private set; }
        public float SecondaryDamageRadius { get; private set; }
        public bool LeechRangeWeapon { get; private set; }
        public float AttackRange { get; private set; }
        public float MinimumAttackRange { get; private set; }
        public int MinTargetPitch { get; private set; }
        public int MaxTargetPitch { get; private set; }
        public DamageType DamageType { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public DamageStatusType DamageStatusType { get; private set; }

        public DeathType DeathType { get; private set; }
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
        public string ProjectileObject { get; private set; }
        public string ProjectileExhaust { get; private set; }
        public string VeterancyProjectileExhaust { get; private set; }
        public string ProjectileStreamName { get; private set; }
        public string FireSound { get; private set; }
        public int FireSoundLoopTime { get; private set; }
        public int SuspendFXDelay { get; private set; }
        public WeaponAffectsTypes RadiusDamageAffects { get; private set; }
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
        public bool AntiSmallMissile { get; private set; }
        public bool AntiProjectile { get; private set; }
        public bool AntiAirborneVehicle { get; private set; }
        public bool AntiAirborneInfantry { get; private set; }
        public bool AntiGround { get; private set; } = true;
        public bool AntiBallisticMissile { get; private set; }
        public bool AntiMine { get; private set; }
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
        public float ShockWaveAmount { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float ShockWaveRadius { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float ShockWaveTaperOff { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool MissileCallsOnDie { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FiringDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ProjectileNugget ProjectileNugget { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public DamageNugget DamageNugget { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public MetaImpactNugget MetaImpactNugget { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MaxWeaponSpeed { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float HitPercentage { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int PreAttackRandomAmount { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IsAimingWeapon { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AntiAirborneMonster { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectKinds FxTrigger { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public SpecialModelConditionNugget SpecialModelConditionNugget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ParalyzeNugget ParalyzeNugget { get; private set; }

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
        public float HitPassengerPercentage { get; private set; }

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
        public bool AntiStructure { get; private set; }

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
        public HordeAttackNugget HordeAttackNugget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public SpawnAndFadeNugget SpawnAndFadeNugget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShareTimers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool DisableScatterForTargetsOnWall { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public AttributeModifierNugget AttributeModifierNugget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShouldPlayUnderAttackEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CanSwoop { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public DamageFieldNugget DamageFieldNugget { get; private set; }

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
        public GrabNugget GrabNugget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RotatingTurret { get; private set; }
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

    [AddedIn(SageGame.Bfme)]
    public class ProjectileNugget
    {
        internal static ProjectileNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProjectileNugget> FieldParseTable = new IniParseTable<ProjectileNugget>
        {
            { "ProjectileTemplateName", (parser, x) => x.ProjectileTemplateName = parser.ParseAssetReference() },
            { "WarheadTemplateName", (parser, x) => x.WarheadTemplateName = parser.ParseAssetReference() },
            { "ForbiddenUpgradeNames", (parser, x) => x.ForbiddenUpgradeNames = parser.ParseAssetReferenceArray() },
            { "RequiredUpgradeNames", (parser, x) => x.RequiredUpgradeNames = parser.ParseAssetReferenceArray() },
            { "SpecialObjectFilter", (parser, x) => x.SpecialObjectFilter = ObjectFilter.Parse(parser) }
        };

        public string ProjectileTemplateName { get; private set; }
        public string WarheadTemplateName { get; private set; }
        public string[] ForbiddenUpgradeNames { get; private set; }
        public string[] RequiredUpgradeNames { get; private set; }
        public ObjectFilter SpecialObjectFilter { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class DamageNugget
    {
        internal static DamageNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DamageNugget> FieldParseTable = new IniParseTable<DamageNugget>
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
            { "SpecialObjectFilter", (parser, x) => x.SpecialObjectFilter = ObjectFilter.Parse(parser) },
            { "DamageMaxHeight", (parser, x) => x.DamageMaxHeight = parser.ParseInteger() },
            { "AcceptDamageAdd", (parser, x) => x.AcceptDamageAdd = parser.ParseBoolean() },
            { "ForbiddenUpgradeNames", (parser, x) => x.ForbiddenUpgradeNames = parser.ParseAssetReferenceArray() },
            { "RequiredUpgradeNames", (parser, x) => x.RequiredUpgradeNames = parser.ParseAssetReferenceArray() },
        };

        public float Damage { get; private set; }
        public float Radius { get; private set; }
        public int DelayTime { get; private set; }
        public DamageType DamageType { get; private set; }
        public FxType DamageFxType { get; private set; }
        public DeathType DeathType { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float DamageSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DamageArc { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public DamageScalar DamageScalar { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter SpecialObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DamageMaxHeight { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AcceptDamageAdd { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] ForbiddenUpgradeNames { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] RequiredUpgradeNames { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class MetaImpactNugget
    {
        internal static MetaImpactNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MetaImpactNugget> FieldParseTable = new IniParseTable<MetaImpactNugget>
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
            { "FlipDirection", (parser, x) => x.FlipDirection = parser.ParseBoolean() }
        };

        public float HeroResist { get; private set; }
        public float ShockWaveAmount { get; private set; }
        public float ShockWaveRadius { get; private set; }
        public float ShockWaveTaperOff { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveArc { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveZMult { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float  ShockWaveSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool InvertShockWave { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DelayTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool FlipDirection { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class SpecialModelConditionNugget
    {
        internal static SpecialModelConditionNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialModelConditionNugget> FieldParseTable = new IniParseTable<SpecialModelConditionNugget>
        {
            { "ModelConditionNames", (parser, x) => x.ModelConditionNames = parser.ParseAssetReferenceArray() },
            { "ModelConditionDuration", (parser, x) => x.ModelConditionDuration = parser.ParseInteger() },
            { "SpecialObjectFilter", (parser, x) => x.SpecialObjectFilter = ObjectFilter.Parse(parser) }
        };

        public string[] ModelConditionNames { get; private set; }
        public int ModelConditionDuration { get; private set; }
        public ObjectFilter SpecialObjectFilter { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class ParalyzeNugget
    {
        internal static ParalyzeNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParalyzeNugget> FieldParseTable = new IniParseTable<ParalyzeNugget>
        {
            { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
            { "Duration", (parser, x) => x.Duration = parser.ParseInteger() }
        };

        public float Radius { get; private set; }
        public int Duration { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class HordeAttackNugget
    {
        internal static HordeAttackNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HordeAttackNugget> FieldParseTable = new IniParseTable<HordeAttackNugget>
        {
        };
    }

    [AddedIn(SageGame.Bfme)]
    public class SpawnAndFadeNugget
    {
        internal static SpawnAndFadeNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpawnAndFadeNugget> FieldParseTable = new IniParseTable<SpawnAndFadeNugget>
        {
            { "ObjectTargetFilter", (parser, x) => x.ObjectTargetFilter = ObjectFilter.Parse(parser) },
            { "SpawnedObjectName", (parser, x) => x.SpawnedObjectName = parser.ParseAssetReference() },
            { "SpawnOffset", (parser, x) => x.SpawnOffset = parser.ParseVector3() }
        };

        public ObjectFilter ObjectTargetFilter {get; private set; } 
        public string SpawnedObjectName {get; private set; } 
        public Vector3 SpawnOffset {get; private set; } 
    }

    [AddedIn(SageGame.Bfme)]
    public class AttributeModifierNugget
    {
        internal static AttributeModifierNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AttributeModifierNugget> FieldParseTable = new IniParseTable<AttributeModifierNugget>
        {
            { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() },
            { "DamageFXType", (parser, x) => x.DamageFxType = parser.ParseEnum<FxType>() },
            { "SpecialObjectFilter", (parser, x) => x.SpecialObjectFilter = ObjectFilter.Parse(parser) }
        };

        public string AttributeModifier { get; private set; }
        public FxType DamageFxType { get; private set; }
        public ObjectFilter SpecialObjectFilter { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class DamageFieldNugget
    {
        internal static DamageFieldNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DamageFieldNugget> FieldParseTable = new IniParseTable<DamageFieldNugget>
        {
            { "WeaponTemplateName", (parser, x) => x.WeaponTemplateName = parser.ParseAssetReference() },
            { "Duration", (parser, x) => x.Duration = parser.ParseInteger() }
        };

        public string WeaponTemplateName { get; private set; }
        public int Duration { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public class GrabNugget
    {
        internal static GrabNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GrabNugget> FieldParseTable = new IniParseTable<GrabNugget>
        {
            { "ContainTargetOnEffect", (parser, x) => x.ContainTargetOnEffect = parser.ParseBoolean() },
            { "ImpactTargetOnEffect", (parser, x) => x.ImpactTargetOnEffect = parser.ParseBoolean() },
            { "ShockWaveAmount", (parser, x) => x.ShockWaveAmount = parser.ParseFloat() },
            { "ShockWaveRadius", (parser, x) => x.ShockWaveRadius = parser.ParseFloat() },
            { "ShockWaveTaperOff", (parser, x) => x.ShockWaveTaperOff = parser.ParseFloat() },
            { "ShockWaveZMult", (parser, x) => x.ShockWaveZMult = parser.ParseFloat() },
        };

        public bool ContainTargetOnEffect { get; private set; }
        public bool ImpactTargetOnEffect { get; private set; }
        public float ShockWaveAmount { get; private set; }
        public float ShockWaveRadius { get; private set; }
        public float ShockWaveTaperOff { get; private set; }
        public float ShockWaveZMult { get; private set; }
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

        public float Scalar { get; private set; }
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
    }

    [Flags]
    public enum WeaponAffectsTypes
    {
        None = 0,

        [IniEnum("SELF")]
        Self = 1 << 0,

        [IniEnum("ALLIES")]
        Allies = 1 << 1,

        [IniEnum("ENEMIES")]
        Enemies = 1 << 2,

        [IniEnum("NEUTRALS")]
        Neutrals = 1 << 3,

        [IniEnum("NOT_SIMILAR")]
        NotSimilar = 1 << 4,

        [IniEnum("SUICIDE")]
        Suicide = 1 << 5,

        [IniEnum("NOT_AIRBORNE")]
        NotAirborne = 1 << 6,

        [IniEnum("SAME_HEIGHT_ONLY")]
        SameHeightOnly = 1 << 6,

        [IniEnum("MINES")]
        Mines = 1 << 7,
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
}
