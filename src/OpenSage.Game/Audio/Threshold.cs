using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini.Parser;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;
using OpenSage.Logic.Object;

namespace OpenSage.Audio
{
    public sealed class Threshold
    {
        internal static Threshold Parse(IniParser parser)
        {
            return parser.ParseIndexedBlock(
                (x, index) => x.NumUnits = index,
                FieldParseTable);
        }

        private static readonly IniParseTable<Threshold> FieldParseTable = new IniParseTable<Threshold>
        {
            { "VoiceAttack", (parser, x) => x.AudioArrayVoice.AudioEntries.Add(new AudioVoiceEntry { AudioType = ThingTemplateVoiceType.VoiceAttack, Sound = parser.ParseAudioEventReference() }) },
            { "VoiceAttackCharge", (parser, x) => x.AudioArrayVoice.AudioEntries.Add(new AudioVoiceEntry { AudioType = ThingTemplateVoiceType.VoiceAttackCharge, Sound = parser.ParseAudioEventReference() }) },
            { "VoiceMove", (parser, x) => x.AudioArrayVoice.AudioEntries.Add(new AudioVoiceEntry { AudioType = ThingTemplateVoiceType.VoiceMove, Sound = parser.ParseAudioEventReference() }) },
            { "VoiceSelect", (parser, x) => x.AudioArrayVoice.AudioEntries.Add(new AudioVoiceEntry { AudioType = ThingTemplateVoiceType.VoiceSelect, Sound = parser.ParseAudioEventReference() }) },
            { "UnitSpecificSounds", (parser, x) => x.UnitSpecificSoundSet.Add(UnitSpecificSounds.Parse(parser)) }
        };

        internal static Threshold ParseAsset(BinaryReader reader, AssetImportCollection imports)
        {
            return new Threshold
            {
                NumUnits = reader.ReadInt32(),
                AudioArrayVoice = AudioArrayVoice.ParseAsset(reader, imports)
            };
        }

        public int NumUnits { get; private set; }

        public string VoiceAttack { get; private set; }
        public string VoiceAttackCharge { get; private set; }
        public string VoiceMove { get; private set; }
        public string VoiceSelect { get; private set; }

        public List<UnitSpecificSounds> UnitSpecificSoundSet { get; } = new List<UnitSpecificSounds>();

        public AudioArrayVoice AudioArrayVoice { get; private set; } = new AudioArrayVoice();
    }
}
