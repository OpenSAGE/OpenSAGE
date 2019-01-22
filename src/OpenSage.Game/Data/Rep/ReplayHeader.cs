using System;
using System.IO;
using System.Text;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Rep
{
    public sealed class ReplayHeader
    {
        public ReplayGameType GameType { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public ushort NumTimecodes { get; private set; }

        public string Filename { get; private set; }

        public ReplayTimestamp Timestamp { get; private set; }

        public string Version { get; private set; }
        public string BuildDate { get; private set; }

        public ushort VersionMinor { get; private set; }
        public ushort VersionMajor { get; private set; }

        // Maybe a hash of... something.
        public byte[] UnknownHash { get; private set; }

        public ReplayMetadata Metadata { get; private set; }

        public ushort Unknown1 { get; private set; }
        public uint Unknown2 { get; private set; }
        public uint Unknown3 { get; private set; }
        public uint Unknown4 { get; private set; }

        public uint GameSpeed { get; private set; }

        internal static ReplayHeader Parse(BinaryReader reader)
        {
            var gameType = ParseGameType(reader.BaseStream);

            var result = new ReplayHeader
            {
                GameType = gameType
            };

            result.StartTime = ReadTimestamp(reader);
            result.EndTime = ReadTimestamp(reader);

            if (gameType == ReplayGameType.Generals)
            {
                result.NumTimecodes = reader.ReadUInt16();

                var zero = reader.ReadBytes(12);
                // TODO
                //for (var i = 0; i < zero.Length; i++)
                //{
                //    if (zero[i] != 0)
                //    {
                //        throw new InvalidDataException();
                //    }
                //}
            }
            else
            {
                throw new NotImplementedException();
            }

            result.Filename = reader.ReadNullTerminatedString();

            result.Timestamp = ReplayTimestamp.Parse(reader);

            result.Version = reader.ReadNullTerminatedString();
            result.BuildDate = reader.ReadNullTerminatedString();

            result.VersionMinor = reader.ReadUInt16();
            result.VersionMajor = reader.ReadUInt16();

            result.UnknownHash = reader.ReadBytes(8);

            result.Metadata = ReplayMetadata.Parse(reader);

            result.Unknown1 = reader.ReadUInt16();

            result.Unknown2 = reader.ReadUInt32();
            result.Unknown3 = reader.ReadUInt32();
            result.Unknown4 = reader.ReadUInt32();

            result.GameSpeed = reader.ReadUInt32();

            return result;
        }

        private static ReplayGameType ParseGameType(Stream stream)
        {
            using (var asciiReader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var gameTypeHeader = asciiReader.ReadFixedLengthString(6);
                if (gameTypeHeader == "GENREP")
                {
                    return ReplayGameType.Generals;
                }
                else
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    gameTypeHeader = asciiReader.ReadFixedLengthString(8);
                    switch (gameTypeHeader)
                    {
                        case "BFMEREPL":
                            return ReplayGameType.Bfme;

                        case "BFME2RPL":
                            return ReplayGameType.Bfme2;

                        default:
                            throw new NotImplementedException("Replay type not yet implemented: " + gameTypeHeader);
                    }
                }
            }
        }

        private static DateTime ReadTimestamp(BinaryReader reader)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(reader.ReadUInt32());
        }
    }
}
