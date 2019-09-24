using System.IO;
using OpenSage.Data;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public sealed class AudioFile : BaseAsset
    {
        internal static AudioFile FromFileSystemEntry(FileSystemEntry entry, string name)
        {
            var result = new AudioFile
            {
                Entry = entry
            };
            result.SetNameAndInstanceId("AudioFile", name);
            return result;
        }

        internal static AudioFile ParseAsset(BinaryReader reader, Asset asset)
        {
            var result = new AudioFile
            {
                Entry = null, // TODO
                Subtitle = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                NumberOfSamples = reader.ReadInt32(),
                SampleRate = reader.ReadInt32(),
                HeaderDataOffset = reader.ReadInt32(),
                HeaderDataSize = reader.ReadInt32(),
                NumberOfChannels = reader.ReadByte()
            };
            result.SetNameAndInstanceId(asset);
            return result;
        }

        public FileSystemEntry Entry { get; internal set; }
        public string Subtitle { get; private set; }
        public byte NumberOfChannels { get; private set; }
        public int NumberOfSamples { get; private set; }
        public int SampleRate { get; private set; }
        public int HeaderDataOffset { get; private set; }
        public int HeaderDataSize { get; private set; }
    }
}
