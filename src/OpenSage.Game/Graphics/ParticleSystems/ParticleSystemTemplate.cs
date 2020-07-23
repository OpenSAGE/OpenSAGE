using System;
using System.IO;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleSystemTemplate : BaseAsset
    {
        internal static ParticleSystemTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("ParticleSystemTemplate", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<ParticleSystemTemplate> FieldParseTable = new IniParseTable<ParticleSystemTemplate>
        {
            { "Priority", (parser, x) => x.Priority = parser.ParseEnum<ParticleSystemPriority>() },
            { "IsOneShot", (parser, x) => x.IsOneShot = parser.ParseBoolean() },
            { "Shader", (parser, x) => x.Shader = parser.ParseEnum<ParticleSystemShader>() },
            { "Type", (parser, x) => x.Type = parser.ParseEnum<ParticleSystemType>() },
            { "ParticleName", (parser, x) => x.Particle = parser.ParseTextureReference() },
            { "AngleX", (parser, x) => x.AngleX = parser.ParseRandomVariable() },
            { "AngleY", (parser, x) => x.AngleY = parser.ParseRandomVariable() },
            { "AngleZ", (parser, x) => x.AngleZ = parser.ParseRandomVariable() },
            { "AngularRateX", (parser, x) => x.AngularRateX = parser.ParseRandomVariable() },
            { "AngularRateY", (parser, x) => x.AngularRateY = parser.ParseRandomVariable() },
            { "AngularRateZ", (parser, x) => x.AngularRateZ = parser.ParseRandomVariable() },
            { "AngularDamping", (parser, x) => x.AngularDamping = parser.ParseRandomVariable() },
            { "VelocityDamping", (parser, x) => x.VelocityDamping = parser.ParseRandomVariable() },
            { "Gravity", (parser, x) => x.Gravity = parser.ParseFloat() },
            { "PerParticleAttachedSystem", (parser, x) => x.PerParticleAttachedSystem = parser.ParseAssetReference() },
            { "SlaveSystem", (parser, x) => x.SlaveSystem = parser.ParseAssetReference() },
            { "SlavePosOffset", (parser, x) => x.SlavePosOffset = parser.ParseVector3() },
            { "Lifetime", (parser, x) => x.Lifetime = parser.ParseRandomVariable() },
            { "SystemLifetime", (parser, x) => x.SystemLifetime = parser.ParseInteger() },
            { "Size", (parser, x) => x.Size = parser.ParseRandomVariable() },
            { "StartSizeRate", (parser, x) => x.StartSizeRate = parser.ParseRandomVariable() },
            { "SizeRate", (parser, x) => x.SizeRate = parser.ParseRandomVariable() },
            { "SizeRateDamping", (parser, x) => x.SizeRateDamping = parser.ParseRandomVariable() },
            { "Alpha1", (parser, x) => x.Alpha1 = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha2", (parser, x) => x.Alpha2 = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha3", (parser, x) => x.Alpha3 = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha4", (parser, x) => x.Alpha4 = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha5", (parser, x) => x.Alpha5 = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha6", (parser, x) => x.Alpha6 = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha7", (parser, x) => x.Alpha7 = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha8", (parser, x) => x.Alpha8 = RandomAlphaKeyframe.Parse(parser) },
            { "Color1", (parser, x) => x.Color1 = RgbColorKeyframe.Parse(parser) },
            { "Color2", (parser, x) => x.Color2 = RgbColorKeyframe.Parse(parser) },
            { "Color3", (parser, x) => x.Color3 = RgbColorKeyframe.Parse(parser) },
            { "Color4", (parser, x) => x.Color4 = RgbColorKeyframe.Parse(parser) },
            { "Color5", (parser, x) => x.Color5 = RgbColorKeyframe.Parse(parser) },
            { "Color6", (parser, x) => x.Color6 = RgbColorKeyframe.Parse(parser) },
            { "Color7", (parser, x) => x.Color7 = RgbColorKeyframe.Parse(parser) },
            { "Color8", (parser, x) => x.Color8 = RgbColorKeyframe.Parse(parser) },
            { "ColorScale", (parser, x) => x.ColorScale = parser.ParseRandomVariable() },
            { "BurstDelay", (parser, x) => x.BurstDelay = parser.ParseRandomVariable() },
            { "BurstCount", (parser, x) => x.BurstCount = parser.ParseRandomVariable() },
            { "InitialDelay", (parser, x) => x.InitialDelay = parser.ParseRandomVariable() },
            { "DriftVelocity", (parser, x) => x.DriftVelocity = parser.ParseVector3() },
            { "VelocityType", (parser, x) => x.VelocityType = parser.ParseEnum<ParticleVelocityType>() },
            { "VelOrthoX", (parser, x) => x.VelOrthoX = parser.ParseRandomVariable() },
            { "VelOrthoY", (parser, x) => x.VelOrthoY = parser.ParseRandomVariable() },
            { "VelOrthoZ", (parser, x) => x.VelOrthoZ = parser.ParseRandomVariable() },
            { "VelHemispherical", (parser, x) => x.VelHemispherical = parser.ParseRandomVariable() },
            { "VelOutward", (parser, x) => x.VelOutward = parser.ParseRandomVariable() },
            { "VelOutwardOther", (parser, x) => x.VelOutwardOther = parser.ParseRandomVariable() },
            { "VelSpherical", (parser, x) => x.VelSpherical = parser.ParseRandomVariable() },
            { "VelCylindricalRadial", (parser, x) => x.VelCylindricalRadial = parser.ParseRandomVariable() },
            { "VelCylindricalNormal", (parser, x) => x.VelCylindricalNormal = parser.ParseRandomVariable() },
            { "VolumeType", (parser, x) => x.VolumeType = parser.ParseEnum<ParticleVolumeType>() },
            { "VolLineStart", (parser, x) => x.VolLineStart = parser.ParseVector3() },
            { "VolLineEnd", (parser, x) => x.VolLineEnd = parser.ParseVector3() },
            { "VolCylinderRadius", (parser, x) => x.VolCylinderRadius = parser.ParseFloat() },
            { "VolCylinderLength", (parser, x) => x.VolCylinderLength = parser.ParseFloat() },
            { "VolSphereRadius", (parser, x) => x.VolSphereRadius = parser.ParseFloat() },
            { "VolBoxHalfSize", (parser, x) => x.VolBoxHalfSize = parser.ParseVector3() },
            { "IsHollow", (parser, x) => x.IsHollow = parser.ParseBoolean() },
            { "IsGroundAligned", (parser, x) => x.IsGroundAligned = parser.ParseBoolean() },
            { "IsEmitAboveGroundOnly", (parser, x) => x.IsEmitAboveGroundOnly = parser.ParseBoolean() },
            { "IsParticleUpTowardsEmitter", (parser, x) => x.IsParticleUpTowardsEmitter = parser.ParseBoolean() },
            { "WindMotion", (parser, x) => x.WindMotion = parser.ParseEnum<ParticleSystemWindMotion>() },
            { "WindStrength", (parser, x) => x.WindStrength = parser.ParseFloat() },
            { "WindFullStrengthDist", (parser, x) => x.WindFullStrengthDist = parser.ParseFloat() },
            { "WindZeroStrengthDist", (parser, x) => x.WindZeroStrengthDist = parser.ParseFloat() },
            { "WindAngleChangeMin", (parser, x) => x.WindAngleChangeMin = parser.ParseFloat() },
            { "WindAngleChangeMax", (parser, x) => x.WindAngleChangeMax = parser.ParseFloat() },
            { "WindPingPongStartAngleMin", (parser, x) => x.WindPingPongStartAngleMin = parser.ParseFloat() },
            { "WindPingPongStartAngleMax", (parser, x) => x.WindPingPongStartAngleMax = parser.ParseFloat() },
            { "WindPingPongEndAngleMin", (parser, x) => x.WindPingPongEndAngleMin = parser.ParseFloat() },
            { "WindPingPongEndAngleMax", (parser, x) => x.WindPingPongEndAngleMax = parser.ParseFloat() }
        };

        public ParticleSystemPriority Priority { get; private set; }
        public bool IsOneShot { get; private set; }
        public ParticleSystemShader Shader { get; private set; }
        public ParticleSystemType Type { get; private set; }
        public LazyAssetReference<TextureAsset> Particle { get; private set; }
        public RandomVariable AngleX { get; private set; }
        public RandomVariable AngleY { get; private set; }
        public RandomVariable AngleZ { get; private set; }
        public RandomVariable AngularRateX { get; private set; }
        public RandomVariable AngularRateY { get; private set; }
        public RandomVariable AngularRateZ { get; private set; }
        public RandomVariable AngularDamping { get; private set; }
        public RandomVariable VelocityDamping { get; private set; }
        public float Gravity { get; private set; }
        public string PerParticleAttachedSystem { get; private set; }
        public string SlaveSystem { get; private set; }
        public Vector3 SlavePosOffset { get; private set; }
        public RandomVariable Lifetime { get; private set; }
        public int SystemLifetime { get; private set; }
        public RandomVariable Size { get; private set; }
        public RandomVariable StartSizeRate { get; private set; }
        public RandomVariable SizeRate { get; private set; }
        public RandomVariable SizeRateDamping { get; private set; }
        public RandomAlphaKeyframe Alpha1 { get; private set; }
        public RandomAlphaKeyframe Alpha2 { get; private set; }
        public RandomAlphaKeyframe Alpha3 { get; private set; }
        public RandomAlphaKeyframe Alpha4 { get; private set; }
        public RandomAlphaKeyframe Alpha5 { get; private set; }
        public RandomAlphaKeyframe Alpha6 { get; private set; }
        public RandomAlphaKeyframe Alpha7 { get; private set; }
        public RandomAlphaKeyframe Alpha8 { get; private set; }
        public RgbColorKeyframe Color1 { get; private set; }
        public RgbColorKeyframe Color2 { get; private set; }
        public RgbColorKeyframe Color3 { get; private set; }
        public RgbColorKeyframe Color4 { get; private set; }
        public RgbColorKeyframe Color5 { get; private set; }
        public RgbColorKeyframe Color6 { get; private set; }
        public RgbColorKeyframe Color7 { get; private set; }
        public RgbColorKeyframe Color8 { get; private set; }
        public RandomVariable ColorScale { get; private set; }
        public RandomVariable BurstDelay { get; private set; }
        public RandomVariable BurstCount { get; private set; }
        public RandomVariable InitialDelay { get; private set; }
        public Vector3 DriftVelocity { get; private set; }
        public ParticleVelocityType VelocityType { get; private set; }
        public RandomVariable VelOrthoX { get; private set; }
        public RandomVariable VelOrthoY { get; private set; }
        public RandomVariable VelOrthoZ { get; private set; }
        public RandomVariable VelHemispherical { get; private set; }
        public RandomVariable VelOutward { get; private set; }
        public RandomVariable VelOutwardOther { get; private set; }
        public RandomVariable VelSpherical { get; private set; }
        public RandomVariable VelCylindricalRadial { get; private set; }
        public RandomVariable VelCylindricalNormal { get; private set; }
        public ParticleVolumeType VolumeType { get; private set; }
        public Vector3 VolLineStart { get; private set; }
        public Vector3 VolLineEnd { get; private set; }
        public float VolCylinderRadius { get; private set; }
        public float VolCylinderLength { get; private set; }
        public float VolSphereRadius { get; private set; }
        public Vector3 VolBoxHalfSize { get; private set; }
        public bool IsHollow { get; private set; }
        public bool IsGroundAligned { get; private set; }
        public bool IsEmitAboveGroundOnly { get; private set; }
        public bool IsParticleUpTowardsEmitter { get; private set; }
        public ParticleSystemWindMotion WindMotion { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float WindStrength { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float WindFullStrengthDist { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float WindZeroStrengthDist { get; private set; }

        public float WindAngleChangeMin { get; private set; }
        public float WindAngleChangeMax { get; private set; }
        public float WindPingPongStartAngleMin { get; private set; }
        public float WindPingPongStartAngleMax { get; private set; }
        public float WindPingPongEndAngleMin { get; private set; }
        public float WindPingPongEndAngleMax { get; private set; }

        public FXParticleSystemTemplate ToFXParticleSystemTemplate()
        {
            FXParticleEmissionVelocityBase GetEmissionVelocity()
            {
                switch (VelocityType)
                {
                    case ParticleVelocityType.None:
                        return null;

                    case ParticleVelocityType.Ortho:
                        return new FXParticleEmissionVelocityOrtho
                        {
                            X = VelOrthoX,
                            Y = VelOrthoY,
                            Z = VelOrthoZ
                        };

                    case ParticleVelocityType.Hemispherical:
                        return new FXParticleEmissionVelocityHemisphere
                        {
                            Speed = VelHemispherical
                        };

                    case ParticleVelocityType.Outward:
                        return new FXParticleEmissionVelocityOutward
                        {
                            Speed = VelOutward,
                            OtherSpeed = VelOutwardOther
                        };

                    case ParticleVelocityType.Spherical:
                        return new FXParticleEmissionVelocitySphere
                        {
                            Speed = VelSpherical
                        };

                    case ParticleVelocityType.Cylindrical:
                        return new FXParticleEmissionVelocityCylinder
                        {
                            Normal = VelCylindricalNormal,
                            Radial = VelCylindricalRadial
                        };

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            FXParticleEmissionVolumeBase GetEmissionVolume()
            {
                switch (VolumeType)
                {
                    case ParticleVolumeType.None:
                        return null;
                        
                    case ParticleVolumeType.Point:
                        return new FXParticleEmissionVolumePoint
                        {
                            IsHollow = IsHollow
                        };

                    case ParticleVolumeType.Line:
                        return new FXParticleEmissionVolumeLine
                        {
                            StartPoint = VolLineStart,
                            EndPoint = VolLineEnd,
                            IsHollow = IsHollow
                        };
                        
                    case ParticleVolumeType.Cylinder:
                        return new FXParticleEmissionVolumeCylinder
                        {
                            Radius = VolCylinderRadius,
                            Length = VolCylinderLength,
                            IsHollow = IsHollow
                        };
                        
                    case ParticleVolumeType.Sphere:
                        return new FXParticleEmissionVolumeSphere
                        {
                            Radius = VolSphereRadius,
                            IsHollow = IsHollow
                        };
                        
                    case ParticleVolumeType.Box:
                        return new FXParticleEmissionVolumeBox
                        {
                            HalfSize = VolBoxHalfSize,
                            IsHollow = IsHollow
                        };

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new FXParticleSystemTemplate(Name)
            {
                Priority = Priority,
                IsOneShot = IsOneShot,
                Shader = Shader,
                Type = Type,
                ParticleTexture = Particle,
                PerParticleAttachedSystem = PerParticleAttachedSystem,
                SlaveSystem = SlaveSystem,
                SlavePosOffset = SlavePosOffset,
                Lifetime = Lifetime,
                SystemLifetime = SystemLifetime,
                Size = Size,
                StartSizeRate = StartSizeRate,
                IsGroundAligned = IsGroundAligned,
                IsEmitAboveGroundOnly = IsEmitAboveGroundOnly,
                IsParticleUpTowardsEmitter = IsParticleUpTowardsEmitter,
                BurstDelay = BurstDelay,
                BurstCount = BurstCount,
                InitialDelay = InitialDelay,
                Colors = new FXParticleColor
                {
                    Color1 = Color1,
                    Color2 = Color2,
                    Color3 = Color3,
                    Color4 = Color4,
                    Color5 = Color5,
                    Color6 = Color6,
                    Color7 = Color7,
                    Color8 = Color8,
                    ColorScale = ColorScale
                },
                Alpha = new FXParticleAlpha
                {
                    Alpha1 = Alpha1,
                    Alpha2 = Alpha2,
                    Alpha3 = Alpha3,
                    Alpha4 = Alpha4,
                    Alpha5 = Alpha5,
                    Alpha6 = Alpha6,
                    Alpha7 = Alpha7,
                    Alpha8 = Alpha8
                },
                Update = new FXParticleUpdateDefault
                {
                    AngleZ = AngleZ,
                    AngularDamping = AngularDamping,
                    AngularRateZ = AngularRateZ,
                    SizeRate = SizeRate,
                    SizeRateDamping = SizeRateDamping
                },
                Physics = new FXParticleDefaultPhysics
                {
                    DriftVelocity = DriftVelocity,
                    Gravity = Gravity,
                    VelocityDamping = VelocityDamping
                },
                Draw = new FXParticleDrawDefault(),
                Wind = new FXParticleWind
                {
                    TurbulenceAmplitude = 0,
                    WindAngleChangeMax = WindAngleChangeMax,
                    WindAngleChangeMin = WindAngleChangeMin,
                    WindFullStrengthDist = WindFullStrengthDist,
                    WindMotion = WindMotion,
                    WindPingPongEndAngleMax = WindPingPongEndAngleMax,
                    WindPingPongEndAngleMin = WindPingPongEndAngleMin,
                    WindPingPongStartAngleMax = WindPingPongStartAngleMax,
                    WindPingPongStartAngleMin = WindPingPongStartAngleMin,
                    WindStrength = WindStrength,
                    WindZeroStrengthDist = WindZeroStrengthDist
                },
                EmissionVelocity = GetEmissionVelocity(),
                EmissionVolume = GetEmissionVolume(),  
            };
        }
    }

    public sealed class RandomAlphaKeyframe
    {
        internal static RandomAlphaKeyframe Parse(IniParser parser)
        {
            return new RandomAlphaKeyframe
            {
                Value = new RandomVariable(
                    parser.ParseFloat(),
                    parser.ParseFloat(),
                    DistributionType.Uniform),
                Time = parser.ParseUnsignedInteger()
            };
        }

        internal static RandomAlphaKeyframe ReadFromSaveFile(BinaryReader reader)
        {
            return new RandomAlphaKeyframe
            {
                Value = reader.ReadRandomVariable(),
                Time = reader.ReadUInt32()
            };
        }

        public RandomVariable Value;
        public uint Time;
    }

    public sealed class RgbColorKeyframe
    {
        internal static RgbColorKeyframe Parse(IniParser parser)
        {
            return new RgbColorKeyframe
            {
                Color = parser.ParseColorRgb().ToColorRgbF(),
                Time = parser.ParseUnsignedInteger()
            };
        }

        internal static RgbColorKeyframe ReadFromSaveFile(BinaryReader reader)
        {
            return new RgbColorKeyframe
            {
                Color = reader.ReadColorRgbF(),
                Time = reader.ReadUInt32()
            };
        }

        public ColorRgbF Color;
        public uint Time;
    }

    public enum ParticleSystemShader
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("ALPHA")]
        Alpha,

        [IniEnum("ALPHA_TEST")]
        AlphaTest,

        [IniEnum("ADDITIVE")]
        Additive,

        [IniEnum("MULTIPLY")]
        Multiply,

        [IniEnum("W3D_EMISSIVE"), AddedIn(SageGame.Bfme)]
        W3dEmissive,

        [IniEnum("W3D_ALPHA"), AddedIn(SageGame.Bfme)]
        W3dAlpha,

        [IniEnum("W3D_DIFFUSE"), AddedIn(SageGame.Bfme)]
        W3dDiffuse,
    }

    public enum ParticleSystemType
    {
        [IniEnum("PARTICLE")]
        Particle,

        [IniEnum("VOLUME_PARTICLE")]
        VolumeParticle,

        [IniEnum("DRAWABLE")]
        Drawable,

        [IniEnum("STREAK")]
        Streak,

        [IniEnum("SMUDGE"), AddedIn(SageGame.Bfme)]
        Smudge,

        [IniEnum("GPU_PARTICLE"), AddedIn(SageGame.Bfme)]
        GpuParticle,

        [IniEnum("GPU_TERRAINFIRE"), AddedIn(SageGame.Bfme)]
        GpuTerrainfire,
    }

    public enum ParticleVelocityType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("ORTHO")]
        Ortho = 1,

        [IniEnum("SPHERICAL")]
        Spherical = 2,

        [IniEnum("HEMISPHERICAL")]
        Hemispherical,

        [IniEnum("CYLINDRICAL")]
        Cylindrical = 4,

        [IniEnum("OUTWARD")]
        Outward = 5
    }

    public enum ParticleVolumeType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("POINT")]
        Point,

        [IniEnum("LINE")]
        Line,

        [IniEnum("BOX")]
        Box,

        [IniEnum("SPHERE")]
        Sphere = 4,

        [IniEnum("CYLINDER")]
        Cylinder = 5,
    }

    public enum ParticleSystemWindMotion
    {
        [IniEnum("Unused")]
        Unused = 1,

        [IniEnum("PingPong")]
        PingPong,

        [IniEnum("Circular"), AddedIn(SageGame.Bfme)]
        Circular
    }
}
