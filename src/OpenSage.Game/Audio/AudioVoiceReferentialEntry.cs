using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public sealed class AudioVoiceReferentialEntry : SoundOrEvaEvent
    {
        public string Name { get; private set; }

        internal static AudioVoiceReferentialEntry ParseAsset(BinaryReader reader, AssetImportCollection imports)
        {
            var result = new AudioVoiceReferentialEntry();

            ParseAsset(reader, result, imports);

            result.Name = reader.ReadUInt32PrefixedAsciiStringAtOffset();

            return result;
        }
    }
}
