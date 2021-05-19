using System;

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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            GameType = reader.ReadEnum<SaveGameType>();
            MapPath = reader.ReadAsciiString();
            Timestamp = reader.ReadDateTime();
            DisplayName = reader.ReadUnicodeString();
            MapFileName = reader.ReadAsciiString();
            Side = reader.ReadAsciiString();
            MissionIndex = reader.ReadUInt32();
        }
    }
}
