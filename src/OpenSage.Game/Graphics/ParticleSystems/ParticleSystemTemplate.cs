using System;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleSystemTemplate : BaseAsset, IPersistableObject
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
            { "Alpha1", (parser, x) => x.AlphaKeyframes[0] = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha2", (parser, x) => x.AlphaKeyframes[1] = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha3", (parser, x) => x.AlphaKeyframes[2] = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha4", (parser, x) => x.AlphaKeyframes[3] = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha5", (parser, x) => x.AlphaKeyframes[4] = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha6", (parser, x) => x.AlphaKeyframes[5] = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha7", (parser, x) => x.AlphaKeyframes[6] = RandomAlphaKeyframe.Parse(parser) },
            { "Alpha8", (parser, x) => x.AlphaKeyframes[7] = RandomAlphaKeyframe.Parse(parser) },
            { "Color1", (parser, x) => x.ColorKeyframes[0] = RgbColorKeyframe.Parse(parser) },
            { "Color2", (parser, x) => x.ColorKeyframes[1] = RgbColorKeyframe.Parse(parser) },
            { "Color3", (parser, x) => x.ColorKeyframes[2] = RgbColorKeyframe.Parse(parser) },
            { "Color4", (parser, x) => x.ColorKeyframes[3] = RgbColorKeyframe.Parse(parser) },
            { "Color5", (parser, x) => x.ColorKeyframes[4] = RgbColorKeyframe.Parse(parser) },
            { "Color6", (parser, x) => x.ColorKeyframes[5] = RgbColorKeyframe.Parse(parser) },
            { "Color7", (parser, x) => x.ColorKeyframes[6] = RgbColorKeyframe.Parse(parser) },
            { "Color8", (parser, x) => x.ColorKeyframes[7] = RgbColorKeyframe.Parse(parser) },
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
        public bool IsOneShot;
        public ParticleSystemShader Shader;
        public ParticleSystemType Type;
        public LazyAssetReference<TextureAsset> Particle { get; private set; }
        public RandomVariable AngleX;
        public RandomVariable AngleY;
        public RandomVariable AngleZ;
        public RandomVariable AngularRateX;
        public RandomVariable AngularRateY;
        public RandomVariable AngularRateZ;
        public RandomVariable AngularDamping;
        public RandomVariable VelocityDamping;
        public float Gravity;
        public string PerParticleAttachedSystem { get; private set; }
        public string SlaveSystem;
        public Vector3 SlavePosOffset { get; private set; }
        public RandomVariable Lifetime;
        public int SystemLifetime;
        public RandomVariable Size;
        public RandomVariable StartSizeRate;
        public RandomVariable SizeRate;
        public RandomVariable SizeRateDamping;
        public readonly RandomAlphaKeyframe[] AlphaKeyframes = new RandomAlphaKeyframe[ParticleSystem.KeyframeCount];
        public readonly RgbColorKeyframe[] ColorKeyframes = new RgbColorKeyframe[ParticleSystem.KeyframeCount];
        public RandomVariable ColorScale;
        public RandomVariable BurstDelay;
        public RandomVariable BurstCount;
        public RandomVariable InitialDelay;
        public Vector3 DriftVelocity;
        public ParticleVelocityType VelocityType;
        public uint Unknown10;
        public RandomVariable VelOrthoX;
        public RandomVariable VelOrthoY;
        public RandomVariable VelOrthoZ;
        public RandomVariable VelHemispherical;
        public RandomVariable VelOutward;
        public RandomVariable VelOutwardOther;
        public RandomVariable VelSpherical;
        public RandomVariable VelCylindricalRadial;
        public RandomVariable VelCylindricalNormal;
        public ParticleVolumeType VolumeType;
        public Vector3 VolLineStart;
        public Vector3 VolLineEnd;
        public float VolCylinderRadius;
        public float VolCylinderLength;
        public float VolSphereRadius;
        public Vector3 VolBoxHalfSize;
        public bool IsHollow { get; private set; }
        public bool IsGroundAligned { get; private set; }
        public bool IsEmitAboveGroundOnly { get; private set; }
        public bool IsParticleUpTowardsEmitter { get; private set; }

        public uint Unknown11;
        public ParticleSystemWindMotion WindMotion;

        [AddedIn(SageGame.Bfme)]
        public float WindStrength { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float WindFullStrengthDist { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float WindZeroStrengthDist { get; private set; }

        public float Unknown12;
        public float Unknown13;
        public float WindAngleChangeMin;
        public float WindAngleChangeMax;
        public float Unknown14;
        public float WindPingPongStartAngleMin;
        public float WindPingPongStartAngleMax;
        public float Unknown15;
        public float WindPingPongEndAngleMin;
        public float WindPingPongEndAngleMax;

        public bool Unknown16;

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

            var colors = new FXParticleColor
            {
                ColorScale = ColorScale
            };
            for (var i = 0; i < ColorKeyframes.Length; i++)
            {
                colors.ColorKeyframes[i] = ColorKeyframes[i];
            }

            var alpha = new FXParticleAlpha();
            for (var i = 0; i < AlphaKeyframes.Length; i++)
            {
                alpha.AlphaKeyframes[i] = AlphaKeyframes[i];
            }

            return new FXParticleSystemTemplate(Name)
            {
                LegacyTemplate = this,

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
                Colors = colors,
                Alpha = alpha,
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

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBoolean("IsOneShot", ref IsOneShot);
            reader.PersistEnum(ref Shader);
            reader.PersistEnum(ref Type);

            var texture = Particle.Value?.Name;
            reader.PersistAsciiString("Texture", ref texture);

            reader.PersistRandomVariable("AngleX", ref AngleX);
            reader.PersistRandomVariable("AngleY", ref AngleY);
            reader.PersistRandomVariable("AngleZ", ref AngleZ);
            reader.PersistRandomVariable("AngularRateX", ref AngularRateX);
            reader.PersistRandomVariable("AngularRateY", ref AngularRateY);
            reader.PersistRandomVariable("AngularRateZ", ref AngularRateZ);
            reader.PersistRandomVariable("AngularDamping", ref AngularDamping);
            reader.PersistRandomVariable("VelocityDamping", ref VelocityDamping);
            reader.PersistRandomVariable("Lifetime", ref Lifetime);
            reader.PersistInt32(ref SystemLifetime);
            reader.PersistRandomVariable("Size", ref Size);
            reader.PersistRandomVariable("StartSizeRate", ref StartSizeRate);
            reader.PersistRandomVariable("SizeRate", ref SizeRate);
            reader.PersistRandomVariable("SizeRateDamping", ref SizeRateDamping);

            reader.PersistArray("AlphaKeyframes", AlphaKeyframes, static (StatePersister persister, ref RandomAlphaKeyframe item) =>
            {
                persister.BeginObject();

                var randomVariable = item.Value;
                persister.PersistRandomVariable("Value", ref randomVariable);

                var time = item.Time;
                persister.PersistUInt32("Time", ref time);

                if (persister.Mode == StatePersistMode.Read)
                {
                    item = new RandomAlphaKeyframe(randomVariable, time);
                }

                persister.EndObject();
            });

            reader.PersistArray("ColorKeyframes", ColorKeyframes, static (StatePersister persister, ref RgbColorKeyframe item) =>
            {
                persister.BeginObject();

                var color = item.Color;
                persister.PersistColorRgbF("Color", ref color);

                var time = item.Time;
                persister.PersistUInt32("Time", ref time);

                if (persister.Mode == StatePersistMode.Read)
                {
                    item = new RgbColorKeyframe(color, time);
                }

                persister.EndObject();
            });

            reader.PersistRandomVariable("ColorScale", ref ColorScale);
            reader.PersistRandomVariable("BurstDelay", ref BurstDelay);
            reader.PersistRandomVariable("BurstCount", ref BurstCount);
            reader.PersistRandomVariable("InitialDelay", ref InitialDelay);
            reader.PersistVector3("DriftVelocity", ref DriftVelocity);
            reader.PersistSingle("Gravity", ref Gravity);
            reader.PersistAsciiString("SlaveSystemName", ref SlaveSystem);

            reader.SkipUnknownBytes(13);

            reader.PersistEnum(ref VelocityType);
            reader.PersistUInt32("Unknown10", ref Unknown10);

            switch (VelocityType)
            {
                case ParticleVelocityType.Ortho:
                    reader.PersistRandomVariable("VelOrthoX", ref VelOrthoX);
                    reader.PersistRandomVariable("VelOrthoY", ref VelOrthoY);
                    reader.PersistRandomVariable("VelOrthoZ", ref VelOrthoZ);
                    break;

                case ParticleVelocityType.Spherical:
                    reader.PersistRandomVariable("VelSpherical", ref VelSpherical);
                    break;

                case ParticleVelocityType.Hemispherical:
                    reader.PersistRandomVariable("VelHemispherical", ref VelHemispherical);
                    break;

                case ParticleVelocityType.Cylindrical:
                    reader.PersistRandomVariable("VelCylindricalRadial", ref VelCylindricalRadial);
                    reader.PersistRandomVariable("VelCylindricalNormal", ref VelCylindricalNormal);
                    break;

                case ParticleVelocityType.Outward:
                    reader.PersistRandomVariable("VelOutward", ref VelOutward);
                    reader.PersistRandomVariable("VelOutwardOther", ref VelOutwardOther);
                    break;

                default:
                    throw new NotImplementedException();
            }

            reader.PersistEnum(ref VolumeType);

            switch (VolumeType)
            {
                case ParticleVolumeType.Point:
                    break;

                case ParticleVolumeType.Line:
                    reader.PersistVector3("VolLineStart", ref VolLineStart);
                    reader.PersistVector3("VolLineEnd", ref VolLineEnd);
                    break;

                case ParticleVolumeType.Box:
                    reader.PersistVector3("VolBoxHalfSize", ref VolBoxHalfSize);
                    break;

                case ParticleVolumeType.Sphere:
                    reader.PersistSingle("VolSphereRadius", ref VolSphereRadius); // Interesting, value doesn't match ini file
                    break;

                case ParticleVolumeType.Cylinder:
                    reader.PersistSingle("VolCylinderRadius", ref VolCylinderRadius);
                    reader.PersistSingle("VolCylinderLength", ref VolCylinderLength);
                    break;

                default:
                    throw new NotImplementedException();
            }

            reader.PersistUInt32("Unknown11", ref Unknown11);
            reader.PersistEnum(ref WindMotion);
            reader.PersistSingle("Unknown12", ref Unknown12);
            reader.PersistSingle("Unknown13", ref Unknown13); // Almost same as WindAngleChangeMin
            reader.PersistSingle("WindAngleChangeMin", ref WindAngleChangeMin);
            reader.PersistSingle("WindAngleChangeMax", ref WindAngleChangeMax);
            reader.PersistSingle("Unknown14", ref Unknown14);
            reader.PersistSingle("WindPingPongStartAngleMin", ref WindPingPongStartAngleMin);
            reader.PersistSingle("WindPingPongStartAngleMax", ref WindPingPongStartAngleMax);
            reader.PersistSingle("Unknown15", ref Unknown15);
            reader.PersistSingle("WindPingPongEndAngleMin", ref WindPingPongEndAngleMin);
            reader.PersistSingle("WindPingPongEndAngleMax", ref WindPingPongEndAngleMax);
            reader.PersistBoolean("Unknown16", ref Unknown16);
        }
    }

    public readonly record struct RandomAlphaKeyframe(RandomVariable Value, uint Time)
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
    }

    public readonly record struct RgbColorKeyframe(ColorRgbF Color, uint Time)
    {
        internal static RgbColorKeyframe Parse(IniParser parser)
        {
            return new RgbColorKeyframe
            {
                Color = parser.ParseColorRgb().ToColorRgbF(),
                Time = parser.ParseUnsignedInteger()
            };
        }
    }

    public enum ParticleSystemShader
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("ADDITIVE")]
        Additive = 1,

        [IniEnum("ALPHA")]
        Alpha = 2,

        [IniEnum("ALPHA_TEST")]
        AlphaTest = 3,

        [IniEnum("MULTIPLY")]
        Multiply = 4,

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
        Particle = 1,

        [IniEnum("DRAWABLE")]
        Drawable = 2,

        [IniEnum("STREAK")]
        Streak = 3,

        [IniEnum("VOLUME_PARTICLE")]
        VolumeParticle = 4,

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
        Hemispherical = 3,

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
        Box = 3,

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
