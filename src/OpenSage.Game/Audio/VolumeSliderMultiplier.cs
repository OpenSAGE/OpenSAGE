using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    [AddedIn(SageGame.Bfme)]
    public class VolumeSliderMultiplier
    {
        internal static VolumeSliderMultiplier Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<VolumeSliderMultiplier> FieldParseTable = new IniParseTable<VolumeSliderMultiplier>
        {
            { "Slider", (parser, x) => x.Slider = parser.ParseEnum<AudioVolumeSlider>() },
            { "Multiplier", (parser, x) => x.Multiplier = parser.ParseInteger() },
        };

        internal static VolumeSliderMultiplier ParseAsset(BinaryReader reader)
        {
            return new VolumeSliderMultiplier
            {
                Slider = reader.ReadInt32AsEnum<AudioVolumeSlider>(),
                Multiplier = reader.ReadSingle()
            };
        }

        public AudioVolumeSlider Slider { get; private set; }
        public float Multiplier { get; private set; }
    }
}
