using OpenSage.Data.Utilities.Extensions;
using System;
using System.IO;

namespace OpenSage.Data.Wav
{
    // Supported WAV compression formats
    enum AudioFormatType : UInt16
    {
        Pcm = 0x01,
        DviAdpcm  = 0x11,
    }

    struct RiffHeader
    {
        public string ChunkId;
        public UInt32 ChunkSize;
        public string Format;

        public static RiffHeader Parse(BinaryReader reader)
        {
            var header = new RiffHeader();
            header.ChunkId = reader.ReadFourCc();
            header.ChunkSize = reader.ReadUInt32();
            header.Format = reader.ReadFourCc();
            if (header.ChunkId != "RIFF" ||
                header.Format != "WAVE")
            {
                throw new InvalidDataException("Invalid or missing .wav file header!");
            }
            return header;
        }
    }

    struct WaveFormat
    {
        public string SubChunkID;
        public UInt32 SubChunkSize;
        public UInt16 AudioFormat;
        public UInt16 NumChannels;
        public UInt32 SampleRate;
        public UInt32 ByteRate;
        public UInt16 BlockAlign;
        public UInt16 BitsPerSample;
        public UInt16 ExtraBytesSize; // Only used in certain compressed formats
        public byte[] ExtraBytes; // Only used in certain compressed formats

        public static WaveFormat Parse(BinaryReader reader)
        {
            var format = new WaveFormat();
            format.SubChunkID = reader.ReadFourCc();
            if (format.SubChunkID != "fmt ")
            {
                throw new InvalidDataException("Invalid or missing .wav file format chunk!");
            }
            format.SubChunkSize = reader.ReadUInt32();
            format.AudioFormat = reader.ReadUInt16();
            format.NumChannels = reader.ReadUInt16();
            format.SampleRate = reader.ReadUInt32();
            format.ByteRate = reader.ReadUInt32();
            format.BlockAlign = reader.ReadUInt16();
            format.BitsPerSample = reader.ReadUInt16();
            switch ((AudioFormatType) format.AudioFormat)
            {
                case AudioFormatType.Pcm:
                    format.ExtraBytesSize = 0;
                    format.ExtraBytes = new byte[0];
                    break;
                case AudioFormatType.DviAdpcm:
                    if (format.NumChannels != 1)
                    {
                        throw new NotSupportedException("Only single channel DVI ADPCM compressed .wavs are supported.");
                    }
                    format.ExtraBytesSize = reader.ReadUInt16();
                    if (format.ExtraBytesSize != 2)
                    {
                        throw new InvalidDataException("Invalid .wav DVI ADPCM format!");
                    }
                    format.ExtraBytes = reader.ReadBytes(format.ExtraBytesSize);
                    break;
                default:
                    throw new NotSupportedException("Invalid or unknown .wav compression format!");
            }
            return format;
        }
    };

    struct WaveFact
    {
        public string SubChunkID;
        public UInt32 SubChunkSize;
        // Technically this chunk could contain arbitrary data. But in practice
        // it only ever contains a single UInt32 representing the number of
        // samples.
        public UInt32 NumSamples;

        public static WaveFact Parse(BinaryReader reader)
        {
            var fact = new WaveFact();
            fact.SubChunkID = reader.ReadFourCc();
            if (fact.SubChunkID != "fact")
            {
                throw new InvalidDataException("Invalid or missing .wav file fact chunk!");
            }
            fact.SubChunkSize = reader.ReadUInt32();
            if (fact.SubChunkSize != 4)
            {
                throw new NotSupportedException("Invalid or unknown .wav compression format!");
            }
            fact.NumSamples = reader.ReadUInt32();
            return fact;
        }
    }

    struct WaveData
    {
        public string SubChunkID; // should contain the word data
        public UInt32 SubChunkSize; // Stores the size of the data block

        public static WaveData Parse(BinaryReader reader)
        {
            var data = new WaveData();
            data.SubChunkID = reader.ReadFourCc();
            if (data.SubChunkID != "data")
            {
                throw new InvalidDataException("Invalid or missing .wav file data chunk!");
            }
            data.SubChunkSize = reader.ReadUInt32();
            return data;
        }
    };

    internal abstract class WavParser
    {
        public abstract byte[] Parse(BinaryReader reader, int size, WaveFormat format);

        public static WavParser GetParser(AudioFormatType type)
        {
            switch(type)
            {
                case AudioFormatType.Pcm: return new PcmParser();
                case AudioFormatType.DviAdpcm: return new DviAdpcmParser();
                default: throw new NotSupportedException("Invalid or unknown .wav compression format!");
            }
        }
    }



    public sealed class WavFile
    {
        private RiffHeader _header;
        private WaveFormat _format;
        private WaveFact _fact;
        private WaveData _data;
        private byte[] _buffer;

        public int Size => _buffer.Length;
        public int Frequency => (int)_format.SampleRate;
        public int AudioFormat => _format.AudioFormat;
        public int Channels => _format.NumChannels;
        public int BitsPerSample => IsCompressed() ? 16 : _format.BitsPerSample;
        public byte[] Buffer => _buffer;

        private bool IsCompressed()
        {
            // Anything not raw PCM is compressed
            return (AudioFormatType) AudioFormat != AudioFormatType.Pcm;
        }

        public void Parse(BinaryReader reader)
        {
            _header = RiffHeader.Parse(reader);
            _format = WaveFormat.Parse(reader);

            // Every format except the original PCM format must have a fact chunk
            if ((AudioFormatType) _format.AudioFormat != AudioFormatType.Pcm)
            {
                _fact = WaveFact.Parse(reader);
            }

            _data = WaveData.Parse(reader);
            _buffer = WavParser
                .GetParser((AudioFormatType)_format.AudioFormat)
                .Parse(reader, (int) _data.SubChunkSize, _format);
        }

        public static WavFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream))
            {
                var wavFile = new WavFile();
                wavFile.Parse(reader);

                return wavFile;
            }
        }
    }
}
