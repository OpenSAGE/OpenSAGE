using System.Collections.Generic;
using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public sealed class AudioArrayVoice
    {
        public List<AudioVoiceEntry> AudioEntries { get; private set; } = new List<AudioVoiceEntry>();
        public AudioObjectSpecificVoiceEntry[] ObjectSpecificEntries { get; private set; }
        public AudioVoiceReferentialEntry[] NamedEntries { get; private set; }

        internal static AudioArrayVoice ParseAsset(BinaryReader reader, AssetImportCollection imports)
        {
            return new AudioArrayVoice
            {
                AudioEntries = reader.ReadListAtOffset(() => AudioVoiceEntry.ParseAsset(reader, imports)),
                ObjectSpecificEntries = reader.ReadArrayAtOffset(() => AudioObjectSpecificVoiceEntry.ParseAsset(reader, imports)),
                NamedEntries = reader.ReadArrayAtOffset(() => AudioVoiceReferentialEntry.ParseAsset(reader, imports)),
            };
        }
    }
}
