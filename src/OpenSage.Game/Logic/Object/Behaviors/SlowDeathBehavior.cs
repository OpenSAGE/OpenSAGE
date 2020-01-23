using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class SlowDeathBehaviorModuleData : UpdateModuleData
    {
        internal static SlowDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SlowDeathBehaviorModuleData> FieldParseTable = new IniParseTable<SlowDeathBehaviorModuleData>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "DeathFlags", (parser, x) => x.DeathFlags = parser.ParseEnumFlags<DeathFlags>() },
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnumBitArray<ObjectStatus>() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnumBitArray<ObjectStatus>() },
            { "ProbabilityModifier", (parser, x) => x.ProbabilityModifier = parser.ParseInteger() },
            { "ModifierBonusPerOverkillPercent", (parser, x) => x.ModifierBonusPerOverkillPercent = parser.ParsePercentage() },
            { "SinkRate", (parser, x) => x.SinkRate = parser.ParseFloat() },
            { "SinkDelay", (parser, x) => x.SinkDelay = parser.ParseInteger() },
            { "SinkDelayVariance", (parser, x) => x.SinkDelayVariance = parser.ParseInteger() },
            { "DestructionDelay", (parser, x) => x.DestructionDelay = parser.ParseInteger() },
            { "DestructionDelayVariance", (parser, x) => x.DestructionDelayVariance = parser.ParseInteger() },
            { "FlingForce", (parser, x) => x.FlingForce = parser.ParseInteger() },
            { "FlingForceVariance", (parser, x) => x.FlingForceVariance = parser.ParseInteger() },
            { "FlingPitch", (parser, x) => x.FlingPitch = parser.ParseInteger() },
            { "FlingPitchVariance", (parser, x) => x.FlingPitchVariance = parser.ParseInteger() },
            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<SlowDeathPhase>()] = parser.ParseAssetReference() },
            { "FX", (parser, x) => x.FXs[parser.ParseEnum<SlowDeathPhase>()] = parser.ParseAssetReference() },
            { "Weapon", (parser, x) => x.Weapons[parser.ParseEnum<SlowDeathPhase>()] = parser.ParseAssetReference() },
            { "FadeDelay", (parser, x) => x.FadeDelay = parser.ParseInteger() },
            { "FadeTime", (parser, x) => x.FadeTime = parser.ParseInteger() },
            { "Sound", (parser, x) => x.Sound = parser.ParseString() },
            { "DecayBeginTime", (parser, x) => x.DecayBeginTime = parser.ParseInteger() },
            { "ShadowWhenDead", (parser, x) => x.ShadowWhenDead = parser.ParseBoolean() },
            { "DoNotRandomizeMidpoint", (parser, x) => x.DoNotRandomizeMidpoint = parser.ParseBoolean() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public BitArray<ObjectStatus> RequiredStatus { get; private set; }
        public BitArray<ObjectStatus> ExemptStatus { get; private set; }
        public int ProbabilityModifier { get; private set; }
        public Percentage ModifierBonusPerOverkillPercent { get; private set; }
        public float SinkRate { get; private set; }
        public int SinkDelay { get; private set; }
        public int SinkDelayVariance { get; private set; }
        public int DestructionDelay { get; private set; }
        public int DestructionDelayVariance { get; private set; }
        public int FlingForce { get; private set; }
        public int FlingForceVariance { get; private set; }
        public int FlingPitch { get; private set; }
        public int FlingPitchVariance { get; private set; }

        public Dictionary<SlowDeathPhase, string> OCLs { get; } = new Dictionary<SlowDeathPhase, string>();
        public Dictionary<SlowDeathPhase, string> FXs { get; } = new Dictionary<SlowDeathPhase, string>();
        public Dictionary<SlowDeathPhase, string> Weapons { get; } = new Dictionary<SlowDeathPhase, string>();

        [AddedIn(SageGame.Bfme)]
        public DeathFlags DeathFlags { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FadeDelay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FadeTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Sound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DecayBeginTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShadowWhenDead { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool DoNotRandomizeMidpoint { get; private set; }
    }

    public enum SlowDeathPhase
    {
        [IniEnum("INITIAL")]
        Initial,

        [IniEnum("MIDPOINT")]
        Midpoint,

        [IniEnum("FINAL")]
        Final,

        [IniEnum("HIT_GROUND")]
        HitGround
    }

    [AddedIn(SageGame.Bfme)]
    [Flags]
    public enum DeathFlags
    {
        None = 0,

        [IniEnum("DEATH_1")]
        Death1 = 1 << 0,

        [IniEnum("DEATH_2")]
        Death2 = 1 << 1,

        [IniEnum("DEATH_3")]
        Death3 = 1 << 2,

        [IniEnum("DEATH_4")]
        Death4 = 1 << 3,

        [IniEnum("DEATH_5")]
        Death5 = 1 << 4,
    }
}
