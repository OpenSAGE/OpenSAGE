using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.Wav
{
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

        public static WaveFormat Parse(BinaryReader reader)
        {
            var format = new WaveFormat();
            format.SubChunkID = reader.ReadChars(4);
            format.SubChunkSize = reader.ReadUInt32();
            format.AudioFormat = reader.ReadUInt16();
            format.NumChannels = reader.ReadUInt16();
            format.SampleRate = reader.ReadUInt32();
            format.ByteRate = reader.ReadUInt32();
            format.BlockAlign = reader.ReadUInt16();
            format.BitsPerSample = reader.ReadUInt16();
            return format;
        }
    };

    struct WaveData
    {
        public char[] SubChunkID; //should contain the word data
        public UInt32 SubChunkSize; //Stores the size of the data block

        public static WaveData Parse(BinaryReader reader)
        {
            var data = new WaveData();
            data.SubChunkID = reader.ReadChars(4);
            data.SubChunkSize = reader.ReadUInt32();
            return data;
        }
    };

    public sealed class WavFile
    {
        private RiffHeader _header;
        private WaveFormat _format;
        private WaveData _data;
        private byte[] _buffer;

        public uint Size => _data.SubChunkSize;
        public uint Fequency => _format.SampleRate;
        public uint Channels => _format.NumChannels;
        public byte[] Buffer => _buffer;

        public void Parse(BinaryReader reader)
        {
            _header = RiffHeader.Parse(reader);

            if (new string(_header.ChunkId) != "RIFF" ||
                new string(_header.Format) != "WAVE")
            {
                throw new InvalidDataException("Invalid .wav file!");
            }

            _format = WaveFormat.Parse(reader);

            if (new string(_format.SubChunkID)!="fmt ")
            {
                throw new InvalidDataException("Invalid .wav file!");
            }

            if (_format.SubChunkSize > 16)
                reader.BaseStream.Seek(2, SeekOrigin.Current);


            _data = WaveData.Parse(reader);

            if (new string(_data.SubChunkID) != "data")
            {
                throw new InvalidDataException("Invalid .wav file!");
            }

            _buffer = reader.ReadBytes((int)_data.SubChunkSize);
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
