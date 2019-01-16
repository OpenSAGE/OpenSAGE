using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public abstract class BaseSingleSound
    {
        public string Name { get; protected set; }

        public float Volume { get; private set; } = 100;
        public int VolumeShift { get; private set; }
        public int MinVolume { get; private set; }
        public int PlayPercent { get; private set; }
        public int Limit { get; private set; }
        public AudioPriority Priority { get; private set; } = AudioPriority.Normal;
        public BitArray<AudioTypeFlags> Type { get; private set; }
        public AudioControlFlags Control { get; private set; }
        public float MinRange { get; private set; }
        public float MaxRange { get; private set; }
        public int LowPassCutoff { get; private set; }
        public int ReverbEffectLevel { get; private set; }
        public int DryLevel { get; private set; }
        public AudioVolumeSlider SubmixSlider { get; private set; }
        public FloatRange PitchShift { get; private set; }
        public IntRange Delay { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float[] PerFileVolumeShift { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float[] PerFilePitchShift { get; private set; }

        internal static readonly IniParseTable<BaseSingleSound> FieldParseTable = new IniParseTable<BaseSingleSound>
        {
            { "Volume", (parser, x) => x.Volume = parser.ParseFloat() },
            { "VolumeShift", (parser, x) => x.VolumeShift = parser.ParseInteger() },
            { "MinVolume", (parser, x) => x.MinVolume = parser.ParseInteger() },
            { "PlayPercent", (parser, x) => x.PlayPercent = parser.ParseInteger() },
            { "Limit", (parser, x) => x.Limit = parser.ParseInteger() },
            { "Priority", (parser, x) => x.Priority = parser.ParseEnum<AudioPriority>() },
            { "Type", (parser, x) => x.Type = parser.ParseEnumBitArray<AudioTypeFlags>() },
            { "Control", (parser, x) => x.Control = parser.ParseEnumFlags<AudioControlFlags>() },
            { "MinRange", (parser, x) => x.MinRange = parser.ParseFloat() },
            { "MaxRange", (parser, x) => x.MaxRange = parser.ParseFloat() },
            { "LowPassCutoff", (parser, x) => x.LowPassCutoff = parser.ParseInteger() },
            { "ReverbEffectLevel", (parser, x) => x.MaxRange = parser.ParseInteger() },
            { "DryLevel", (parser, x) => x.MaxRange = parser.ParseInteger() },
            { "SubmixSlider", (parser, x) => x.SubmixSlider = parser.ParseEnum<AudioVolumeSlider>() },
            { "PitchShift", (parser, x) => x.PitchShift = FloatRange.Parse(parser) },
            { "Delay", (parser, x) => x.Delay = IntRange.Parse(parser) },
            { "PerFileVolumeShift", (parser, x) => x.PerFileVolumeShift = parser.ParseFloatArray() },
            { "PerFilePitchShift", (parser, x) => x.PerFilePitchShift = parser.ParseFloatArray() },
        };
    }

    public sealed class AudioEvent : BaseSingleSound
    {
        internal static AudioEvent Parse(IniParser parser)
        {
            var audioEvent = parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);

            // HACK for Generals: In order to know which sounds to localise, we need to check if the event was loaded from Voice.ini.
            // Most localised sounds have the Voice audio type flag, but many don't, so we need to make sure the flag is set.
            if (parser.CurrentPosition.File.EndsWith("Voice.ini"))
            {
                audioEvent.Type?.Set(AudioTypeFlags.Voice, true);
            }

            return audioEvent;
        }

        private static new readonly IniParseTable<AudioEvent> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<AudioEvent>
            {
                { "Sounds", (parser, x) => x.Sounds = parser.ParseAssetReferenceArray() },
                { "Attack", (parser, x) => x.Attack = parser.ParseAssetReferenceArray() },
                { "Decay", (parser, x) => x.Decay = parser.ParseAssetReferenceArray() },
                { "LoopCount", (parser, x) => x.LoopCount = parser.ParseInteger() },
                { "VolumeSliderMultiplier", (parser, x) => x.VolumeSliderMultipliers.Add(VolumeSliderMultiplier.Parse(parser)) },
                { "ZoomedInOffscreenVolumePercent", (parser, x) => x.ZoomedInOffscreenVolumePercent = parser.ParsePercentage() },
                { "ZoomedInOffscreenMinVolumePercent", (parser, x) => x.ZoomedInOffscreenMinVolumePercent = parser.ParsePercentage() },
                { "ZoomedInOffscreenOcclusionPercent", (parser, x) => x.ZoomedInOffscreenOcclusionPercent = parser.ParsePercentage() },
            });

        public string[] Sounds { get; private set; }
        public string[] Attack { get; private set; }
        public string[] Decay { get; private set; }
        public int LoopCount { get; private set; }
        public List<VolumeSliderMultiplier> VolumeSliderMultipliers { get; } = new List<VolumeSliderMultiplier>();

        [AddedIn(SageGame.Bfme2)]
        public float ZoomedInOffscreenVolumePercent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ZoomedInOffscreenMinVolumePercent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ZoomedInOffscreenOcclusionPercent { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class StreamedSound : BaseSingleSound
    {
        internal static StreamedSound Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static new readonly IniParseTable<StreamedSound> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<StreamedSound>
            {
                { "Filename", (parser, x) => x.Filename = parser.ParseAssetReference() },
            });

        public string Filename { get; private set; }
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

    [AddedIn(SageGame.Bfme)]
    public class VolumeSliderMultiplier
    {
        internal static VolumeSliderMultiplier Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<VolumeSliderMultiplier> FieldParseTable = new IniParseTable<VolumeSliderMultiplier>
        {
            { "Slider", (parser, x) => x.Slider = parser.ParseIdentifier() },
            { "Multiplier", (parser, x) => x.Multiplier = parser.ParseInteger() },
        };

        public string Slider { get; private set; }
        public int Multiplier { get; private set; }
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

    public enum AudioTypeFlags
    {
        [IniEnum("default")]
        Default,

        [IniEnum("world")]
        World,

        [IniEnum("shrouded")]
        Shrouded,

        [IniEnum("everyone")]
        Everyone,

        [IniEnum("ui")]
        UI,

        [IniEnum("player")]
        Player,

        [IniEnum("global")]
        Global,

        [IniEnum("voice")]
        Voice,

        [IniEnum("enemies")]
        Enemies,

        [IniEnum("allies")]
        Allies,

        [IniEnum("FAKE"), AddedIn(SageGame.Bfme)]
        Fake,
    }

    [Flags]
    public enum AudioControlFlags
    {
        [IniEnum("none")]
        None = 0,

        [IniEnum("loop")]
        Loop = 1 << 0,

        [IniEnum("all")]
        All = 1 << 1,

        [IniEnum("interrupt")]
        Interrupt = 1 << 2,

        [IniEnum("random")]
        Random = 1 << 3,

        [IniEnum("randomstart")]
        RandomStart = 1 << 4,

        [IniEnum("fade_on_kill")]
        FadeOnKill = 1 << 5,

        [IniEnum("fade_on_start")]
        FadeOnStart = 1 << 6,

        [IniEnum("sequential"), AddedIn(SageGame.Bfme2)]
        Sequential = 1 << 7,
    }
}
