using System;
using System.IO;
using System.Text;

namespace OpenSage.Data.Bik
{
    // Based on file format notes here:
    // https://wiki.multimedia.cx/index.php/Bink_Container
    // https://wiki.multimedia.cx/index.php/Bink_Video
    // https://wiki.multimedia.cx/index.php/Bink_Audio
    public sealed class BinkFile : IDisposable
    {
        private readonly BinaryReader _reader;
        private int _currentFrame;

        public BinkFileHeader Header { get; private set; }
        public BinkFrameIndexTable FrameIndexTable { get; private set; }

        private BinkFile(BinaryReader reader)
        {
            _reader = reader;
        }

        public static BinkFile FromFileSystemEntry(FileSystemEntry entry)
        {
            var stream = entry.Open();
            var reader = new BinaryReader(stream, Encoding.ASCII, false);

            var result = new BinkFile(reader);

            result.Header = BinkFileHeader.Parse(reader);
            result.FrameIndexTable = BinkFrameIndexTable.Parse(reader, result.Header.NumFrames);

            return result;
        }

        public BinkFrame ReadFrame()
        {
            ref var frameIndex = ref FrameIndexTable.Indices[_currentFrame];
            var offset = frameIndex.Offset;

            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var result = BinkFrame.Parse(_reader, Header, FrameIndexTable.Indices[_currentFrame + 1].Offset - offset);

            _currentFrame += 1;

            return result;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }

    public sealed class BinkFrame
    {
        internal static BinkFrame Parse(BinaryReader reader, BinkFileHeader header, uint frameLength)
        {
            var startPosition = reader.BaseStream.Position;

            for (var i = 0; i < header.NumAudioTracks; i++)
            {
                var audioPacketLength = reader.ReadUInt32();
                var numSamples = reader.ReadUInt32();

                // TODO
                if (audioPacketLength > 0)
                {
                    reader.ReadBytes((int) (audioPacketLength - 4));
                }
            }

            // TODO
            reader.ReadBytes((int) (frameLength - (reader.BaseStream.Position - startPosition)));

            return new BinkFrame();
        }
    }

    public sealed class BinkAudioPacket
    {
        internal static BinkAudioPacket Parse(BinaryReader reader)
        {
            return new BinkAudioPacket();
        }
    }

    public sealed class BinkFrameIndexTable
    {
        public BinkFrameIndex[] Indices { get; private set; }

        internal static BinkFrameIndexTable Parse(BinaryReader reader, uint numFrames)
        {
            var result = new BinkFrameIndexTable();
            result.Indices = new BinkFrameIndex[numFrames + 1];

            for (var i = 0; i < numFrames + 1; i++)
            {
                var offset = reader.ReadUInt32();

                if ((offset & 1) == 1)
                {
                    result.Indices[i].IsKeyframe = true;
                    offset &= ~1u;
                }

                result.Indices[i].Offset = offset;
            }

            return result;
        }
    }

    public struct BinkFrameIndex
    {
        public uint Offset;
        public bool IsKeyframe;
    }

    public sealed class BinkFileHeader
    {
        public uint FileSizeExcludingFirst8Bytes { get; private set; }
        public uint NumFrames { get; private set; }
        public uint LargestFrameSizeBytes { get; private set; }
        public uint VideoWidth { get; private set; }
        public uint VideoHeight { get; private set; }
        public uint VideoFpsDividend { get; private set; }
        public uint VideoFpsDivider { get; private set; }
        public BinkVideoFlags VideoFlags { get; private set; }
        public uint NumAudioTracks { get; private set; }

        public BinkAudioTrackHeader[] AudioTracks { get; private set; }

        internal static BinkFileHeader Parse(BinaryReader reader)
        {
            var signature = new string(reader.ReadChars(3));
            if (signature != "BIK")
            {
                throw new InvalidDataException();
            }

            var revision = reader.ReadChar();
            if (revision != 'i')
            {
                throw new NotSupportedException();
            }

            var result = new BinkFileHeader
            {
                FileSizeExcludingFirst8Bytes = reader.ReadUInt32(),
                NumFrames = reader.ReadUInt32(),
                LargestFrameSizeBytes = reader.ReadUInt32()
            };

            var numFramesRepeat = reader.ReadUInt32();
            if (numFramesRepeat != result.NumFrames)
            {
                throw new InvalidDataException();
            }

            result.VideoWidth = reader.ReadUInt32();
            result.VideoHeight = reader.ReadUInt32();
            result.VideoFpsDividend = reader.ReadUInt32();
            result.VideoFpsDivider = reader.ReadUInt32();

            result.VideoFlags = (BinkVideoFlags) reader.ReadUInt32();
            if (result.VideoFlags != BinkVideoFlags.None)
            {
                throw new NotSupportedException();
            }

            result.NumAudioTracks = reader.ReadUInt32();
            if (result.NumAudioTracks > 1)
            {
                throw new NotSupportedException();
            }

            result.AudioTracks = new BinkAudioTrackHeader[result.NumAudioTracks];

            for (var i = 0; i < result.NumAudioTracks; i++)
            {
                result.AudioTracks[i] = new BinkAudioTrackHeader();

                var unknown = reader.ReadUInt16();
                result.AudioTracks[i].NumChannels = reader.ReadUInt16();
            }

            for (var i = 0; i < result.NumAudioTracks; i++)
            {
                result.AudioTracks[i].SampleRate = reader.ReadUInt16();
                result.AudioTracks[i].Flags = (BinkAudioFlags) reader.ReadUInt16();

                if (!result.AudioTracks[i].Flags.HasFlag(BinkAudioFlags.BinkAudioFft))
                {
                    throw new NotSupportedException();
                }
            }

            for (var i = 0; i < result.NumAudioTracks; i++)
            {
                result.AudioTracks[i].TrackID = reader.ReadUInt32();
            }

            return result;
        }
    }

    public sealed class BinkAudioTrackHeader
    {
        public ushort NumChannels { get; internal set; }
        public ushort SampleRate { get; internal set; }
        public BinkAudioFlags Flags { get; internal set; }
        public uint TrackID { get; internal set; }
    }

    [Flags]
    public enum BinkVideoFlags
    {
        None = 0,

        Grayscale = 1 << 17,

        HasAlphaPlane = 1 << 20,

        TwoTimesHeightDoubled = 1 << 28,
        TwoTimesHeightInterlaced = 2 << 28,
        TwoTimesWidthDoubled = 3 << 28,
        TwoTimesWidthAndHeightDoubled = 4 << 28,
        TwoTimesWidthAndHeightInterlaced = 5 << 28
    }

    [Flags]
    public enum BinkAudioFlags : ushort
    {
        None = 0,

        BinkAudioDct = 0 << 12,
        BinkAudioFft = 1 << 12,

        Stereo = 1 << 13,

        Unknown1 = 1 << 14,
        Unknown2 = 1 << 15
    }
}
