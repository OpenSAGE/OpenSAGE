using System;

namespace OpenSage.Data.Sav
{
    public sealed class GameState
    {
        private SaveGameType _gameType;

        public SaveGameType GameType => _gameType;
        public string MapPath { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string DisplayName { get; private set; }
        public string MapFileName { get; private set; }
        public string Side { get; private set; }
        public uint MissionIndex { get; private set; }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            reader.ReadEnum(ref _gameType);
            MapPath = reader.ReadAsciiString();
            Timestamp = reader.ReadDateTime();
            DisplayName = reader.ReadUnicodeString();
            MapFileName = reader.ReadAsciiString();
            Side = reader.ReadAsciiString();
            MissionIndex = reader.ReadUInt32();
        }
    }
}
