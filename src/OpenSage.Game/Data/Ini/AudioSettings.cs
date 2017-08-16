using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class AudioSettings
    {
        internal static AudioSettings Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<AudioSettings> FieldParseTable = new IniParseTable<AudioSettings>
        {
            { "AudioRoot", (parser, x) => x.AudioRoot = parser.ParseString() },
            { "SoundsFolder", (parser, x) => x.SoundsFolder = parser.ParseString() },
            { "MusicFolder", (parser, x) => x.MusicFolder = parser.ParseString() },
            { "StreamingFolder", (parser, x) => x.StreamingFolder = parser.ParseString() },
            { "SoundsExtension", (parser, x) => x.SoundsExtension = parser.ParseString() },
            { "UseDigital", (parser, x) => x.UseDigital = parser.ParseBoolean() },
            { "UseMidi", (parser, x) => x.UseMidi = parser.ParseBoolean() },
            { "OutputRate", (parser, x) => x.OutputRate = parser.ParseInteger() },
            { "OutputBits", (parser, x) => x.OutputBits = parser.ParseInteger() },
            { "OutputChannels", (parser, x) => x.OutputChannels = parser.ParseInteger() },
            { "SampleCount2D", (parser, x) => x.SampleCount2D = parser.ParseInteger() },
            { "SampleCount3D", (parser, x) => x.SampleCount3D = parser.ParseInteger() },
            { "StreamCount", (parser, x) => x.StreamCount = parser.ParseInteger() },
            { "GlobalMinRange", (parser, x) => x.GlobalMinRange = parser.ParseFloat() },
            { "GlobalMaxRange", (parser, x) => x.GlobalMaxRange = parser.ParseFloat() },
            { "TimeBetweenDrawableSounds", (parser, x) => x.TimeBetweenDrawableSounds = parser.ParseInteger() },
            { "TimeToFadeAudio", (parser, x) => x.TimeToFadeAudio = parser.ParseInteger() },
            { "AudioFootprintInBytes", (parser, x) => x.AudioFootprintInBytes = parser.ParseInteger() },
            { "MinSampleVolume", (parser, x) => x.MinSampleVolume = parser.ParseInteger() },

            { "Relative2DVolume", (parser, x) => x.Relative2DVolume = parser.ParsePercentage() },

            { "DefaultSoundVolume", (parser, x) => x.DefaultSoundVolume = parser.ParsePercentage() },
            { "Default3DSoundVolume", (parser, x) => x.Default3DSoundVolume = parser.ParsePercentage() },
            { "DefaultSpeechVolume", (parser, x) => x.DefaultSpeechVolume = parser.ParsePercentage() },
            { "DefaultMusicVolume", (parser, x) => x.DefaultMusicVolume = parser.ParsePercentage() },
            { "Default2DSpeakerType", (parser, x) => x.Default2DSpeakerType = parser.ParseString() },
            { "Default3DSpeakerType", (parser, x) => x.Default3DSpeakerType = parser.ParseString() },

            { "Preferred3DHW1", (parser, x) => x.Preferred3DHW1 = parser.ParseString() },
            { "Preferred3DHW2", (parser, x) => x.Preferred3DHW2 = parser.ParseString() },
            { "Preferred3DSW", (parser, x) => x.Preferred3DSW = parser.ParseString() },

            { "MicrophoneDesiredHeightAboveTerrain", (parser, x) => x.MicrophoneDesiredHeightAboveTerrain = parser.ParseFloat() },
            { "MicrophoneMaxPercentageBetweenGroundAndCamera", (parser, x) => x.MicrophoneMaxPercentageBetweenGroundAndCamera = parser.ParsePercentage() },

            { "ZoomMinDistance", (parser, x) => x.ZoomMinDistance = parser.ParseFloat() },
            { "ZoomMaxDistance", (parser, x) => x.ZoomMaxDistance = parser.ParseFloat() },

            { "ZoomSoundVolumePercentageAmount", (parser, x) => x.ZoomSoundVolumePercentageAmount = parser.ParsePercentage() },
        };

        public string AudioRoot { get; private set; }
        public string SoundsFolder { get; private set; }
        public string MusicFolder { get; private set; }
        public string StreamingFolder { get; private set; }
        public string SoundsExtension { get; private set; }
        public bool UseDigital { get; private set; }
        public bool UseMidi { get; private set; }
        public int OutputRate { get; private set; }
        public int OutputBits { get; private set; }
        public int OutputChannels { get; private set; }
        public int SampleCount2D { get; private set; }
        public int SampleCount3D { get; private set; }
        public int StreamCount { get; private set; }
        public float GlobalMinRange { get; private set; }
        public float GlobalMaxRange { get; private set; }
        public int TimeBetweenDrawableSounds { get; private set; }
        public int TimeToFadeAudio { get; private set; }
        public int AudioFootprintInBytes { get; private set; }
        public int MinSampleVolume { get; private set; }

        public float Relative2DVolume { get; private set; }

        public float DefaultSoundVolume { get; private set; }
        public float Default3DSoundVolume { get; private set; }
        public float DefaultSpeechVolume { get; private set; }
        public float DefaultMusicVolume { get; private set; }
        public string Default2DSpeakerType { get; private set; }
        public string Default3DSpeakerType { get; private set; }

        public string Preferred3DHW1 { get; private set; }
        public string Preferred3DHW2 { get; private set; }
        public string Preferred3DSW { get; private set; }

        public float MicrophoneDesiredHeightAboveTerrain { get; private set; }

        public float MicrophoneMaxPercentageBetweenGroundAndCamera { get; private set; }

        public float ZoomMinDistance { get; private set; }
        public float ZoomMaxDistance { get; private set; }

        public float ZoomSoundVolumePercentageAmount { get; private set; }
    }
}
