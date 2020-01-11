using System;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Lod
{
    public sealed class DynamicGameLod : BaseAsset
    {
        internal static DynamicGameLod Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) =>
                {
                    x.Level = (LodType) Enum.Parse(typeof(LodType), name);
                    x.SetNameAndInstanceId("DynamicGameLod", x.Level.ToString());
                },
                FieldParseTable);
        }

        private static readonly IniParseTable<DynamicGameLod> FieldParseTable = new IniParseTable<DynamicGameLod>
        {
            { "MinimumFPS", (parser, x) => x.MinimumFps = parser.ParseInteger() },
            { "ParticleSkipMask", (parser, x) => x.ParticleSkipMask = parser.ParseInteger() },
            { "DebrisSkipMask", (parser, x) => x.DebrisSkipMask = parser.ParseInteger() },
            { "SlowDeathScale", (parser, x) => x.SlowDeathScale = parser.ParseFloat() },
            { "MinParticlePriority", (parser, x) => x.MinParticlePriority = parser.ParseEnum<ParticleSystemPriority>() },
            { "MinParticleSkipPriority", (parser, x) => x.MinParticleSkipPriority = parser.ParseEnum<ParticleSystemPriority>() },
        };

        public LodType Level { get; private set; }

        public int MinimumFps { get; private set; }
        public int ParticleSkipMask { get; private set; }
        public int DebrisSkipMask { get; private set; }
        public float SlowDeathScale { get; private set; }
        public ParticleSystemPriority MinParticlePriority { get; private set; }
        public ParticleSystemPriority MinParticleSkipPriority { get; private set; }
    }
}
