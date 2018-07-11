using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.Wav
{
    // Supported WAV compression formats
    enum AudioFormatType : UInt16
    {
        Microsoft = 0x01,
        DviAdpcm  = 0x11,
    }

    struct RiffHeader
    {
        public char[] ChunkId;
        public UInt32 ChunkSize;
        public char[] Format;

        public static RiffHeader Parse(BinaryReader reader)
        {
            var header = new RiffHeader();
            header.ChunkId = reader.ReadChars(4);
            header.ChunkSize = reader.ReadUInt32();
            header.Format = reader.ReadChars(4);
            if (new string(header.ChunkId) != "RIFF" ||
                new string(header.Format) != "WAVE")
            {
                throw new InvalidDataException("Invalid or missing .wav file header!");
            }
            return header;
        }
    }

    struct WaveFormat
    {
        public char[] SubChunkID;
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
            format.SubChunkID = reader.ReadChars(4);
            if (new string(format.SubChunkID) != "fmt ")
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
                case AudioFormatType.Microsoft:
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
        public char[] SubChunkID;
        public UInt32 SubChunkSize;
        // Technically this chunk could contain arbitrary data. But in practice
        // it only ever contains a single UInt32 representing the number of
        // samples.
        public UInt32 NumSamples;

        public static WaveFact Parse(BinaryReader reader)
        {
            var fact = new WaveFact();
            fact.SubChunkID = reader.ReadChars(4);
            if (new string(fact.SubChunkID) != "fact")
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
        public char[] SubChunkID; //should contain the word data
        public UInt32 SubChunkSize; //Stores the size of the data block

        public static WaveData Parse(BinaryReader reader)
        {
            var data = new WaveData();
            data.SubChunkID = reader.ReadChars(4);
            if (new string(data.SubChunkID) != "data")
            {
                throw new InvalidDataException("Invalid or missing .wav file data chunk!");
            }
            data.SubChunkSize = reader.ReadUInt32();
            return data;
        }
    };

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
        public int BitsPerSample => _format.BitsPerSample;
        public byte[] Buffer => _buffer;

        private int ByteToInt16(byte[] packed)
        {
            // This is always little endian, unlike the C# builtin for unpacking a byte array.
            return (packed[1] << 8) | packed[0];
        }

        private byte[] ParseMicrosoft(BinaryReader reader, int size)
        {
            return reader.ReadBytes(size);
        }
        
        private byte[] ParseDviAdpcm(BinaryReader reader, int size, int samplesPerBlock)
        {
            var imaIndexTable = new int[16] {
              -1, -1, -1, -1, 2, 4, 6, 8,
              -1, -1, -1, -1, 2, 4, 6, 8
            };
            var imaStepTable = new int[89] {
              7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
              19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
              50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
              130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
              337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
              876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
              2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
              5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
              15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
            };
            int blockSize = (4 + (samplesPerBlock - 1) / 2);
            if (size % blockSize != 0)
            {
                throw new InvalidDataException("Invalid .wav DVI ADPCM data!");
            }
            int numBlocks = size / blockSize;
            var buffer = new byte[samplesPerBlock * 2 * numBlocks];
            using (var memoryStream = new MemoryStream(buffer))
            using (var writer = new BinaryWriter(memoryStream))
            {
                for (int i = 0; i < numBlocks; i++)
                {
                    int sample = reader.ReadInt16();
                    int stepTableIndex = reader.ReadByte();
                    byte reserved = reader.ReadByte(); // unused, commonly 0
                    int step = imaStepTable[stepTableIndex];
                    writer.Write((Int16) sample);
                    for (int j = 0; j < (samplesPerBlock - 1) / 2; j++)
                    {
                        byte packed = reader.ReadByte();
                        byte nibble_low = (byte) (packed & 0xF);
                        byte nibble_high = (byte) (packed >> 4);
                        foreach (var nibble in new byte[] { nibble_low , nibble_high })
                        {
                            stepTableIndex += imaIndexTable[nibble];
                            if (stepTableIndex > 88) stepTableIndex = 88;
                            if (stepTableIndex < 0) stepTableIndex = 0;
                            byte sign = (byte) (nibble & 8);
                            byte delta = (byte) (nibble & 7);
                            int diff = step >> 3;
                            if ((delta & 4) != 0) diff += step;
                            if ((delta & 2) != 0) diff += (step >> 1);
                            if ((delta & 1) != 0) diff += (step >> 2);
                            if (sign != 0) sample -= diff;
                            else sample += diff;
                            step = imaStepTable[stepTableIndex];
                            if (sample > 0xFFFF) sample = 0xFFFF;
                            if (sample < -0xFFFF) sample = -0xFFFF;
                            writer.Write((Int16) sample);
                        }
                    }
                }
            }
            return buffer;
        }

        public void Parse(BinaryReader reader)
        {
            _header = RiffHeader.Parse(reader);
            _format = WaveFormat.Parse(reader);

            // Every format except the original Microsoft format must have a fact chunk
            if ((AudioFormatType) _format.AudioFormat != AudioFormatType.Microsoft)
            {
                _fact = WaveFact.Parse(reader);
            }

            _data = WaveData.Parse(reader);
            switch ((AudioFormatType) _format.AudioFormat)
            {
                case AudioFormatType.Microsoft:
                    _buffer = ParseMicrosoft(reader, (int) _data.SubChunkSize);
                    break;
                case AudioFormatType.DviAdpcm:
                    _buffer = ParseDviAdpcm(reader,
                                            (int) _data.SubChunkSize,
                                            ByteToInt16(_format.ExtraBytes));
                    break;
                default:
                    // Should never happen (since it should throw on parse)
                    _buffer = new byte[0];
                    throw new NotSupportedException("Invalid or unknown .wav compression format!");
            }
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
