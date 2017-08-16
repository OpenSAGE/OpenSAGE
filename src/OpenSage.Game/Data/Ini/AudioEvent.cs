using System;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class AudioEvent
    {
        internal static AudioEvent Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AudioEvent> FieldParseTable = new IniParseTable<AudioEvent>
        {
            { "Priority", (parser, x) => x.Priority = parser.ParseEnum<AudioPriority>() },
            { "Control", (parser, x) => x.Control = parser.ParseEnumFlags<AudioControlFlags>() },
            { "Sounds", (parser, x) => x.Sounds = parser.ParseAssetReferenceArray() },
            { "Attack", (parser, x) => x.Attack = parser.ParseAssetReferenceArray() },
            { "Decay", (parser, x) => x.Decay = parser.ParseAssetReferenceArray() },
            { "PitchShift", (parser, x) => x.PitchShift = FloatRange.Parse(parser) },
            { "Delay", (parser, x) => x.Delay = IntRange.Parse(parser) },
            { "LoopCount", (parser, x) => x.LoopCount = parser.ParseInteger() },
            { "VolumeShift", (parser, x) => x.VolumeShift = parser.ParseInteger() },
            { "Volume", (parser, x) => x.Volume = parser.ParseInteger() },
            { "MinVolume", (parser, x) => x.MinVolume = parser.ParseInteger() },
            { "LowPassCutoff", (parser, x) => x.LowPassCutoff = parser.ParseInteger() },
            { "Limit", (parser, x) => x.Limit = parser.ParseInteger() },
            { "MinRange", (parser, x) => x.MinRange = parser.ParseFloat() },
            { "MaxRange", (parser, x) => x.MaxRange = parser.ParseFloat() },
            { "Type", (parser, x) => x.Type = parser.ParseEnumFlags<AudioTypeFlags>() },
        };

        public string Name { get; private set; }

        public AudioPriority Priority { get; private set; } = AudioPriority.Normal;
        public AudioControlFlags Control { get; private set; }
        public string[] Sounds { get; private set; }
        public string[] Attack { get; private set; }
        public string[] Decay { get; private set; }
        public FloatRange PitchShift { get; private set; }
        public IntRange Delay { get; private set; }
        public int LoopCount { get; private set; }
        public int VolumeShift { get; private set; }
        public int Volume { get; private set; } = 100;
        public int MinVolume { get; private set; }
        public int LowPassCutoff { get; private set; }
        public int Limit { get; private set; }
        public float MinRange { get; private set; }
        public float MaxRange { get; private set; }
        public AudioTypeFlags Type { get; private set; }
    }

    public struct IntRange
    {
        internal static IntRange Parse(IniParser parser)
        {
            return new IntRange
            {
                Low = parser.ParseInteger(),
                High = parser.ParseInteger()
            };
        }

        public int Low { get; private set; }
        public int High { get; private set; }
    }

    public struct FloatRange
    {
        internal static FloatRange Parse(IniParser parser)
        {
            return new FloatRange
            {
                Low = parser.ParseFloat(),
                High = parser.ParseFloat()
            };
        }

        public float Low { get; private set; }
        public float High { get; private set; }
    }

    public enum AudioPriority
    {
        [IniEnum("lowest")]
        Lowest,

        [IniEnum("low")]
        Low,

        [IniEnum("normal")]
        Normal,

        [IniEnum("high")]
        High,

        [IniEnum("critical")]
        Critical
    }

    [Flags]
    public enum AudioTypeFlags
    {
        None = 0,

        [IniEnum("world")]
        World = 1 << 0,

        [IniEnum("shrouded")]
        Shrouded = 1 << 1,

        [IniEnum("everyone")]
        Everyone = 1 << 2,

        [IniEnum("ui")]
        UI = 1 << 3,

        [IniEnum("player")]
        Player = 1 << 4,

        [IniEnum("global")]
        Global = 1 << 5,

        [IniEnum("voice")]
        Voice = 1 << 6
    }

    [Flags]
    public enum AudioControlFlags
    {
        None = 0,

        [IniEnum("loop")]
        Loop = 1 << 0,

        [IniEnum("all")]
        All = 1 << 1,

        [IniEnum("interrupt")]
        Interrupt = 1 << 2,

        [IniEnum("random")]
        Random = 1 << 3
    }
}
