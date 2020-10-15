using System;
using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Sav
{
    public sealed class GameState
    {
        public SaveGameType GameType { get; private set; }
        public string MapPath { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string DisplayName { get; private set; }
        public string MapFileName { get; private set; }
        public string Side { get; private set; }
        public uint MissionIndex { get; private set; }

        internal static GameState Parse(BinaryReader reader)
        {
            return new GameState
            {
                GameType = reader.ReadUInt32AsEnum<SaveGameType>(),
                MapPath = reader.ReadBytePrefixedAsciiString(),
                Timestamp = reader.ReadDateTime(),
                DisplayName = reader.ReadBytePrefixedUnicodeString(),
                MapFileName = reader.ReadBytePrefixedAsciiString(),
                Side = reader.ReadBytePrefixedAsciiString(),
                MissionIndex = reader.ReadUInt32()
            };
        }
    }
}
