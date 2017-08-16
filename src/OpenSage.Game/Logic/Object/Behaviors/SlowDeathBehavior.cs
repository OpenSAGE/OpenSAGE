using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SlowDeathBehavior : ObjectBehavior
    {
        internal static SlowDeathBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SlowDeathBehavior> FieldParseTable = new IniParseTable<SlowDeathBehavior>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "SinkRate", (parser, x) => x.SinkRate = parser.ParseInteger() },
            { "SinkDelay", (parser, x) => x.SinkDelay = parser.ParseInteger() },
            { "DestructionDelay", (parser, x) => x.DestructionDelay = parser.ParseInteger() },
            { "DestructionDelayVariance", (parser, x) => x.DestructionDelayVariance = parser.ParseInteger() },

            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<SlowDeathStage>()] = parser.ParseAssetReference() },
            { "FX", (parser, x) => x.FXs[parser.ParseEnum<SlowDeathStage>()] = parser.ParseAssetReference() },
            { "Weapon", (parser, x) => x.Weapons[parser.ParseEnum<SlowDeathStage>()] = parser.ParseAssetReference() },
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public int SinkRate { get; private set; }
        public int SinkDelay { get; private set; }
        public int DestructionDelay { get; private set; }
        public int DestructionDelayVariance { get; private set; }

        public Dictionary<SlowDeathStage, string> OCLs { get; } = new Dictionary<SlowDeathStage, string>();
        public Dictionary<SlowDeathStage, string> FXs { get; } = new Dictionary<SlowDeathStage, string>();
        public Dictionary<SlowDeathStage, string> Weapons { get; } = new Dictionary<SlowDeathStage, string>();
    }

    public enum SlowDeathStage
    {
        [IniEnum("INITIAL")]
        Initial,

        [IniEnum("FINAL")]
        Final
    }
}
