using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Audio
{
    public abstract class BaseSingleSound : BaseAudioEventInfo
    {
        internal static readonly IniParseTable<BaseSingleSound> FieldParseTable = new IniParseTable<BaseSingleSound>
        {
            { "Volume", (parser, x) => x.Volume = parser.ParsePercentage() },
            { "VolumeShift", (parser, x) => x.VolumeShift = parser.ParsePercentage() },
            { "MinVolume", (parser, x) => x.MinVolume = parser.ParsePercentage() },
            { "PlayPercent", (parser, x) => x.PlayPercent = parser.ParsePercentage() },
            { "Limit", (parser, x) => x.Limit = parser.ParseInteger() },
            { "Priority", (parser, x) => x.Priority = parser.ParseEnum<AudioPriority>() },
            { "Type", (parser, x) => x.Type = parser.ParseEnumFlags<AudioTypeFlags>() },
            { "Control", (parser, x) => x.Control = parser.ParseEnumFlags<AudioControlFlags>() },
            { "MinRange", (parser, x) => x.MinRange = parser.ParseFloat() },
            { "MaxRange", (parser, x) => x.MaxRange = parser.ParseFloat() },
            { "LowPassCutoff", (parser, x) => x.LowPassCutoff = parser.ParsePercentage() },
            { "ZoomedInOffscreenVolumePercent", (parser, x) => x.ZoomedInOffscreenVolumePercent = parser.ParsePercentage() },
            { "ZoomedInOffscreenMinVolumePercent", (parser, x) => x.ZoomedInOffscreenMinVolumePercent = parser.ParsePercentage() },
            { "ZoomedInOffscreenOcclusionPercent", (parser, x) => x.ZoomedInOffscreenOcclusionPercent = parser.ParsePercentage() },
            { "ReverbEffectLevel", (parser, x) => x.ReverbEffectLevel = parser.ParsePercentage() },
            { "DryLevel", (parser, x) => x.DryLevel = parser.ParsePercentage() },
            { "SubmixSlider", (parser, x) => x.SubmixSlider = parser.ParseEnum<AudioVolumeSlider>() },
            { "PitchShift", (parser, x) => x.PitchShift = parser.ParseFloatRange() },
            { "Delay", (parser, x) => x.Delay = parser.ParseIntRange() },
            { "PerFileVolumeShift", (parser, x) => x.PerFileVolumeShift = parser.ParsePercentage() },
            { "PerFilePitchShift", (parser, x) => x.PerFilePitchShift = parser.ParseFloatRange() },
            { "VolumeSliderMultiplier", (parser, x) => x.VolumeSliderMultipliers.Add(VolumeSliderMultiplier.Parse(parser)) },
        };

        private protected static void ParseAsset<T>(BinaryReader reader, T result)
            where T : BaseSingleSound
        {
            result.Volume = reader.ReadPercentage();
            result.VolumeShift = reader.ReadPercentage();
            result.PerFileVolumeShift = reader.ReadPercentage();
            result.MinVolume = reader.ReadPercentage();
            result.PlayPercent = reader.ReadPercentage();
            result.Limit = reader.ReadInt32();
            result.Priority = reader.ReadInt32AsEnum<AudioPriority>();
            result.Type = reader.ReadUInt32AsEnumFlags<AudioTypeFlags>();
            result.Control = reader.ReadUInt32AsEnumFlags<AudioControlFlags>();
            result.MinRange = reader.ReadSingle();
            result.MaxRange = reader.ReadSingle();
            result.LowPassCutoff = reader.ReadPercentage();
            result.ZoomedInOffscreenVolumePercent = reader.ReadPercentage();
            result.ZoomedInOffscreenMinVolumePercent = reader.ReadPercentage();
            result.ZoomedInOffscreenOcclusionPercent = reader.ReadPercentage();
            result.ReverbEffectLevel = reader.ReadPercentage();
            result.DryLevel = reader.ReadPercentage();
            result.SubmixSlider = reader.ReadOptionalValueAtOffset(() => reader.ReadUInt32AsEnum<AudioVolumeSlider>());
            result.PitchShift = reader.ReadOptionalValueAtOffset(() => reader.ReadFloatRange());
            result.PerFilePitchShift = reader.ReadOptionalValueAtOffset(() => reader.ReadFloatRange());
            result.Delay = reader.ReadOptionalValueAtOffset(() => reader.ReadIntRange());
            result.VolumeSliderMultipliers = reader.ReadListAtOffset(() => VolumeSliderMultiplier.ParseAsset(reader));
        }

        public Percentage Volume { get; private set; } = new Percentage(1);
        public Percentage VolumeShift { get; private set; }
        public Percentage MinVolume { get; private set; }
        public Percentage PlayPercent { get; private set; }
        public int Limit { get; private set; }
        public AudioPriority Priority { get; private set; } = AudioPriority.Normal;
        public AudioTypeFlags Type { get; internal set; } // TODO: Make this private.
        public AudioControlFlags Control { get; private set; }
        public float MinRange { get; private set; }
        public float MaxRange { get; private set; }
        public Percentage LowPassCutoff { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage ZoomedInOffscreenVolumePercent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage ZoomedInOffscreenMinVolumePercent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage ZoomedInOffscreenOcclusionPercent { get; private set; }
        public Percentage ReverbEffectLevel { get; private set; }
        public Percentage DryLevel { get; private set; }
        public AudioVolumeSlider? SubmixSlider { get; private set; }
        public FloatRange? PitchShift { get; private set; }
        public IntRange? Delay { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage PerFileVolumeShift { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public FloatRange? PerFilePitchShift { get; private set; }

        public List<VolumeSliderMultiplier> VolumeSliderMultipliers { get; private set; } = new List<VolumeSliderMultiplier>();
    }
}
