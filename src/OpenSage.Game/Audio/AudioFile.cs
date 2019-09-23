using System.IO;
using OpenSage.Data;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public sealed class AudioFile
    {
        internal static AudioFile ParseAsset(BinaryReader reader, Asset asset)
        {
            return new AudioFile
            {
                Name = asset.Name,
                Entry = null, // TODO
                Subtitle = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                NumberOfSamples = reader.ReadInt32(),
                SampleRate = reader.ReadInt32(),
                HeaderDataOffset = reader.ReadInt32(),
                HeaderDataSize = reader.ReadInt32(),
                NumberOfChannels = reader.ReadByte()
            };
        }

        public string Name { get; internal set; }
        public FileSystemEntry Entry { get; internal set; }
        public string Subtitle { get; private set; }
        public byte NumberOfChannels { get; private set; }
        public int NumberOfSamples { get; private set; }
        public int SampleRate { get; private set; }
        public int HeaderDataOffset { get; private set; }
        public int HeaderDataSize { get; private set; }
    }
}
