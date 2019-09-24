using System;
using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AudioLod : BaseAsset
    {
        internal static AudioLod Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) =>
                {
                    x.SetNameAndInstanceId("AudioLOD", name);
                    x.Level = (AudioLodType) Enum.Parse(typeof(AudioLodType), name);
                },
                FieldParseTable);
        }

        private static readonly IniParseTable<AudioLod> FieldParseTable = new IniParseTable<AudioLod>
        {
            { "AllowDolby", (parser, x) => x.AllowDolby = parser.ParseBoolean() },
            { "MaximumAmbientStreams", (parser, x) => x.MaximumAmbientStreams = parser.ParseInteger() },
            { "AllowReverb", (parser, x) => x.AllowReverb = parser.ParseBoolean() }
        };

        public AudioLodType Level { get; private set; }

        public bool AllowDolby { get; private set; }
        public int MaximumAmbientStreams { get; private set; }
        public bool AllowReverb { get; private set; }
    }
}
