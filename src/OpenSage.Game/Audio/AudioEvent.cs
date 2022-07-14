using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    public sealed class AudioEvent : BaseSingleSound
    {
        internal static AudioEvent Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("AudioEvent", name),
                FieldParseTable);
        }

        private static new readonly IniParseTable<AudioEvent> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<AudioEvent>
            {
                { "Sounds", (parser, x) => x.Sounds = parser.ParseAudioFileWithWeightArray() },
                { "Attack", (parser, x) => x.Attack = parser.ParseAudioFileWithWeightArray() },
                { "Decay", (parser, x) => x.Decay = parser.ParseAudioFileWithWeightArray() },
                { "LoopCount", (parser, x) => x.LoopCount = parser.ParseInteger() },
            });

        public AudioFileWithWeight[] Sounds { get; private set; }
        public AudioFileWithWeight[] Attack { get; private set; }
        public AudioFileWithWeight[] Decay { get; private set; }
        public int LoopCount { get; private set; }
    }

    public sealed class AudioFileWithWeight
    {
        public LazyAssetReference<AudioFile> AudioFile { get; internal set; }
    }
}
