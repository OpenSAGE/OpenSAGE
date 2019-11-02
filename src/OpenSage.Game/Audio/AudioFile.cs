using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public sealed class AudioFile : BaseAsset
    {
        internal static AudioFile FromUrl(string url, string name)
        {
            var result = new AudioFile
            {
                Url = url
            };
            result.SetNameAndInstanceId("AudioFile", name);
            return result;
        }

        internal static AudioFile ParseAsset(BinaryReader reader, Asset asset)
        {
            var result = new AudioFile
            {
                Url = null, // TODO
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

        public string Url { get; private set; }
        public string Subtitle { get; private set; }
        public byte NumberOfChannels { get; private set; }
        public int NumberOfSamples { get; private set; }
        public int SampleRate { get; private set; }
        public int HeaderDataOffset { get; private set; }
        public int HeaderDataSize { get; private set; }
    }
}
