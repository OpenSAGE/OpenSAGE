using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

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
            { "ScatterTarget", (parser, x) => x.ScatterTargets.Add(Coord2D.Parse(parser)) },
            { "SecondaryDamage", (parser, x) => x.SecondaryDamage = parser.ParseFloat() },
            { "SecondaryDamageRadius", (parser, x) => x.SecondaryDamageRadius = parser.ParseFloat() },
            { "LeechRangeWeapon", (parser, x) => x.LeechRangeWeapon = parser.ParseBoolean() },
            { "AttackRange", (parser, x) => x.AttackRange = parser.ParseFloat() },
            { "MinimumAttackRange", (parser, x) => x.MinimumAttackRange = parser.ParseFloat() },
            { "MinTargetPitch", (parser, x) => x.MinTargetPitch = parser.ParseInteger() },
            { "MaxTargetPitch", (parser, x) => x.MaxTargetPitch = parser.ParseInteger() },
            { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() },
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
            { "ClipReloadTime", (parser, x) => x.ClipReloadTime = parser.ParseInteger() },
            { "AutoReloadWhenIdle", (parser, x) => x.AutoReloadWhenIdle = parser.ParseInteger() },
            { "AutoReloadsClip", (parser, x) => x.AutoReloadsClip = parser.ParseEnum<WeaponReloadType>() },
            { "ContinuousFireOne", (parser, x) => x.ContinuousFireOne = parser.ParseInteger() },
            { "ContinuousFireTwo", (parser, x) => x.ContinuousFireTwo = parser.ParseInteger() },
            { "ContinuousFireCoast", (parser, x) => x.ContinuousFireCoast = parser.ParseInteger() },
            { "PreAttackDelay", (parser, x) => x.PreAttackDelay = parser.ParseInteger() },
            { "PreAttackType", (parser, x) => x.PreAttackType = parser.ParseEnum<WeaponPrefireType>() },
            { "ContinueAttackRange", (parser, x) => x.ContinueAttackRange = parser.ParseInteger() },
            { "AcceptableAimDelta", (parser, x) => x.AcceptableAimDelta = parser.ParseInteger() },
            { "AntiSmallMissile", (parser, x) => x.AntiSmallMissile = parser.ParseBoolean() },
            { "AntiProjectile", (parser, x) => x.AntiProjectile = parser.ParseBoolean() },
            { "AntiAirborneVehicle", (parser, x) => x.AntiAirborneVehicle = parser.ParseBoolean() },
            { "AntiAirborneInfantry", (parser, x) => x.AntiAirborneInfantry = parser.ParseBoolean() },
            { "AntiGround", (parser, x) => x.AntiGround = parser.ParseBoolean() },
            { "AntiBallisticMissile", (parser, x) => x.AntiBallisticMissile = parser.ParseBoolean() },
            { "AntiMine", (parser, x) => x.AntiMine = parser.ParseBoolean() },
            { "ShowsAmmoPips", (parser, x) => x.ShowsAmmoPips = parser.ParseBoolean() },
            { "LaserName", (parser, x) => x.LaserName = parser.ParseAssetReference() },
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

        public string Name { get; private set; }

        public float PrimaryDamage { get; private set; }
        public float PrimaryDamageRadius { get; private set; }
        public float ScatterRadius { get; private set; }
        public float ScatterRadiusVsInfantry { get; private set; }
        public float ScatterTargetScalar { get; private set; }
        public List<Coord2D> ScatterTargets { get; } = new List<Coord2D>();
        public float SecondaryDamage { get; private set; }
        public float SecondaryDamageRadius { get; private set; }
        public bool LeechRangeWeapon { get; private set; }
        public float AttackRange { get; private set; }
        public float MinimumAttackRange { get; private set; }
        public int MinTargetPitch { get; private set; }
        public int MaxTargetPitch { get; private set; }
        public DamageType DamageType { get; private set; }
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
        public int ClipReloadTime { get; private set; }
        public int AutoReloadWhenIdle { get; private set; }
        public WeaponReloadType AutoReloadsClip { get; private set; } = WeaponReloadType.Auto;
        public int ContinuousFireOne { get; private set; }
        public int ContinuousFireTwo { get; private set; }
        public int ContinuousFireCoast { get; private set; }
        public int PreAttackDelay { get; private set; }
        public WeaponPrefireType PreAttackType { get; private set; }
        public int ContinueAttackRange { get; private set; }
        public int AcceptableAimDelta { get; private set; }
        public bool AntiSmallMissile { get; private set; }
        public bool AntiProjectile { get; private set; }
        public bool AntiAirborneVehicle { get; private set; }
        public bool AntiAirborneInfantry { get; private set; }
        public bool AntiGround { get; private set; } = true;
        public bool AntiBallisticMissile { get; private set; }
        public bool AntiMine { get; private set; }
        public bool ShowsAmmoPips { get; private set; }
        public string LaserName { get; private set; }
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
    }

    public struct RangeDuration
    {
        internal static RangeDuration Parse(IniParser parser)
        {
            if (parser.CurrentTokenType == IniTokenType.IntegerLiteral)
            {
                var value = parser.ParseInteger();
                return new RangeDuration
                {
                    Min = value,
                    Max = value
                };
            }
            return new RangeDuration
            {
                Min = parser.ParseAttributeInteger("Min"),
                Max = parser.ParseAttributeInteger("Max")
            };
        }

        public int Min { get; private set; }
        public int Max { get; private set; }
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
        PerShot
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

        [IniEnum("CRUSHED")]
        Crushed,

        [IniEnum("TOPPLED")]
        Toppled,
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
        NotAirborne = 1 << 6
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
        Shrubbery = 1 << 3
    }
}
