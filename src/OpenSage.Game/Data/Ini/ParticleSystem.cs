using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class ParticleSystemDefinition
    {
        internal static ParticleSystemDefinition Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ParticleSystemDefinition> FieldParseTable = new IniParseTable<ParticleSystemDefinition>
        {
            { "Priority", (parser, x) => x.Priority = parser.ParseEnum<ParticleSystemPriority>() },
            { "IsOneShot", (parser, x) => x.IsOneShot = parser.ParseBoolean() },
            { "Shader", (parser, x) => x.Shader = parser.ParseEnum<ParticleSystemShader>() },
            { "Type", (parser, x) => x.Type = parser.ParseEnum<ParticleSystemType>() },
            { "ParticleName", (parser, x) => x.ParticleName = parser.ParseFileName() },
            { "AngleX", (parser, x) => x.AngleX = RandomVariable.Parse(parser) },
            { "AngleY", (parser, x) => x.AngleY = RandomVariable.Parse(parser) },
            { "AngleZ", (parser, x) => x.AngleZ = RandomVariable.Parse(parser) },
            { "AngularRateX", (parser, x) => x.AngularRateX = RandomVariable.Parse(parser) },
            { "AngularRateY", (parser, x) => x.AngularRateY = RandomVariable.Parse(parser) },
            { "AngularRateZ", (parser, x) => x.AngularRateZ = RandomVariable.Parse(parser) },
            { "AngularDamping", (parser, x) => x.AngularDamping = RandomVariable.Parse(parser) },
            { "VelocityDamping", (parser, x) => x.VelocityDamping = RandomVariable.Parse(parser) },
            { "Gravity", (parser, x) => x.Gravity = parser.ParseFloat() },
            { "PerParticleAttachedSystem", (parser, x) => x.PerParticleAttachedSystem = parser.ParseAssetReference() },
            { "SlaveSystem", (parser, x) => x.SlaveSystem = parser.ParseAssetReference() },
            { "SlavePosOffset", (parser, x) => x.SlavePosOffset = Coord3D.Parse(parser) },
            { "Lifetime", (parser, x) => x.Lifetime = RandomVariable.Parse(parser) },
            { "SystemLifetime", (parser, x) => x.SystemLifetime = parser.ParseInteger() },
            { "Size", (parser, x) => x.Size = RandomVariable.Parse(parser) },
            { "StartSizeRate", (parser, x) => x.StartSizeRate = RandomVariable.Parse(parser) },
            { "SizeRate", (parser, x) => x.SizeRate = RandomVariable.Parse(parser) },
            { "SizeRateDamping", (parser, x) => x.SizeRateDamping = RandomVariable.Parse(parser) },
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
            { "ColorScale", (parser, x) => x.ColorScale = RandomVariable.Parse(parser) },
            { "BurstDelay", (parser, x) => x.BurstDelay = RandomVariable.Parse(parser) },
            { "BurstCount", (parser, x) => x.BurstCount = RandomVariable.Parse(parser) },
            { "InitialDelay", (parser, x) => x.InitialDelay = RandomVariable.Parse(parser) },
            { "DriftVelocity", (parser, x) => x.DriftVelocity = Coord3D.Parse(parser) },
            { "VelocityType", (parser, x) => x.VelocityType = parser.ParseEnum<ParticleVelocityType>() },
            { "VelOrthoX", (parser, x) => x.VelOrthoX = RandomVariable.Parse(parser) },
            { "VelOrthoY", (parser, x) => x.VelOrthoY = RandomVariable.Parse(parser) },
            { "VelOrthoZ", (parser, x) => x.VelOrthoZ = RandomVariable.Parse(parser) },
            { "VelHemispherical", (parser, x) => x.VelHemispherical = RandomVariable.Parse(parser) },
            { "VelOutward", (parser, x) => x.VelOutward = RandomVariable.Parse(parser) },
            { "VelOutwardOther", (parser, x) => x.VelOutwardOther = RandomVariable.Parse(parser) },
            { "VelSpherical", (parser, x) => x.VelSpherical = RandomVariable.Parse(parser) },
            { "VelCylindricalRadial", (parser, x) => x.VelCylindricalRadial = RandomVariable.Parse(parser) },
            { "VelCylindricalNormal", (parser, x) => x.VelCylindricalNormal = RandomVariable.Parse(parser) },
            { "VolumeType", (parser, x) => x.VolumeType = parser.ParseEnum<ParticleVolumeType>() },
            { "VolLineStart", (parser, x) => x.VolLineStart = Coord3D.Parse(parser) },
            { "VolLineEnd", (parser, x) => x.VolLineEnd = Coord3D.Parse(parser) },
            { "VolCylinderRadius", (parser, x) => x.VolCylinderRadius = parser.ParseFloat() },
            { "VolCylinderLength", (parser, x) => x.VolCylinderLength = parser.ParseFloat() },
            { "VolSphereRadius", (parser, x) => x.VolSphereRadius = parser.ParseFloat() },
            { "VolBoxHalfSize", (parser, x) => x.VolBoxHalfSize = Coord3D.Parse(parser) },
            { "IsHollow", (parser, x) => x.IsHollow = parser.ParseBoolean() },
            { "IsGroundAligned", (parser, x) => x.IsGroundAligned = parser.ParseBoolean() },
            { "IsEmitAboveGroundOnly", (parser, x) => x.IsEmitAboveGroundOnly = parser.ParseBoolean() },
            { "IsParticleUpTowardsEmitter", (parser, x) => x.IsParticleUpTowardsEmitter = parser.ParseBoolean() },
            { "WindMotion", (parser, x) => x.WindMotion = parser.ParseEnum<ParticleSystemWindMotion>() },
            { "WindAngleChangeMin", (parser, x) => x.WindAngleChangeMin = parser.ParseFloat() },
            { "WindAngleChangeMax", (parser, x) => x.WindAngleChangeMax = parser.ParseFloat() },
            { "WindPingPongStartAngleMin", (parser, x) => x.WindPingPongStartAngleMin = parser.ParseFloat() },
            { "WindPingPongStartAngleMax", (parser, x) => x.WindPingPongStartAngleMax = parser.ParseFloat() },
            { "WindPingPongEndAngleMin", (parser, x) => x.WindPingPongEndAngleMin = parser.ParseFloat() },
            { "WindPingPongEndAngleMax", (parser, x) => x.WindPingPongEndAngleMax = parser.ParseFloat() }
        };

        public string Name { get; private set; }

        public ParticleSystemPriority Priority { get; private set; }
        public bool IsOneShot { get; private set; }
        public ParticleSystemShader Shader { get; private set; }
        public ParticleSystemType Type { get; private set; }
        public string ParticleName { get; private set; }
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
        public Coord3D SlavePosOffset { get; private set; }
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
        public Coord3D DriftVelocity { get; private set; }
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
        public Coord3D VolLineStart { get; private set; }
        public Coord3D VolLineEnd { get; private set; }
        public float VolCylinderRadius { get; private set; }
        public float VolCylinderLength { get; private set; }
        public float VolSphereRadius { get; private set; }
        public Coord3D VolBoxHalfSize { get; private set; }
        public bool IsHollow { get; private set; }
        public bool IsGroundAligned { get; private set; }
        public bool IsEmitAboveGroundOnly { get; private set; }
        public bool IsParticleUpTowardsEmitter { get; private set; }
        public ParticleSystemWindMotion WindMotion { get; private set; }
        public float WindAngleChangeMin { get; private set; }
        public float WindAngleChangeMax { get; private set; }
        public float WindPingPongStartAngleMin { get; private set; }
        public float WindPingPongStartAngleMax { get; private set; }
        public float WindPingPongEndAngleMin { get; private set; }
        public float WindPingPongEndAngleMax { get; private set; }
    }

    public struct RandomAlphaKeyframe
    {
        internal static RandomAlphaKeyframe Parse(IniParser parser)
        {
            return new RandomAlphaKeyframe
            {
                Low = parser.ParseFloat(),
                High = parser.ParseFloat(),
                Time = parser.ParseInteger()
            };
        }

        public float Low;
        public float High;
        public int Time;
    }

    public struct RgbColorKeyframe
    {
        internal static RgbColorKeyframe Parse(IniParser parser)
        {
            return new RgbColorKeyframe
            {
                Color = IniColorRgb.Parse(parser),
                Time = parser.ParseInteger()
            };
        }

        public IniColorRgb Color;
        public int Time;
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
        Multiply
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
        Streak
    }

    public enum ParticleVelocityType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("ORTHO")]
        Ortho,

        [IniEnum("HEMISPHERICAL")]
        Hemispherical,

        [IniEnum("OUTWARD")]
        Outward,

        [IniEnum("SPHERICAL")]
        Spherical,

        [IniEnum("CYLINDRICAL")]
        Cylindrical
    }

    public enum ParticleVolumeType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("POINT")]
        Point,

        [IniEnum("LINE")]
        Line,

        [IniEnum("CYLINDER")]
        Cylinder,

        [IniEnum("SPHERE")]
        Sphere,

        [IniEnum("BOX")]
        Box
    }

    public enum ParticleSystemWindMotion
    {
        [IniEnum("Unused")]
        Unused,

        [IniEnum("PingPong")]
        PingPong
    }
}
