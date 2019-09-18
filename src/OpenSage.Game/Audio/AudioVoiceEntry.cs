using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public sealed class AudioVoiceEntry : SoundOrEvaEvent
    {
        public ThingTemplateVoiceType AudioType { get; internal set; }

        internal static AudioVoiceEntry ParseAsset(BinaryReader reader, AssetImportCollection imports)
        {
            var result = new AudioVoiceEntry();

            ParseAsset(reader, result, imports);

            result.AudioType = reader.ReadUInt32AsEnum<ThingTemplateVoiceType>();

            return result;
        }
    }
}
