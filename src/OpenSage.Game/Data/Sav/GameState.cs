using System;

namespace OpenSage.Data.Sav
{
    public sealed class GameState
    {
        public SaveGameType GameType;
        public string MapPath;
        public DateTime Timestamp;
        public string DisplayName;
        public string MapFileName;
        public string Side;
        public uint MissionIndex;

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.PersistEnum(ref GameType);
            reader.PersistAsciiString("MapPath", ref MapPath);
            reader.PersistDateTime("Timestamp", ref Timestamp);
            reader.PersistUnicodeString("DisplayName", ref DisplayName);
            reader.PersistAsciiString("MapFileName", ref MapFileName);
            reader.PersistAsciiString("Side", ref Side);
            reader.PersistUInt32("MissionIndex", ref MissionIndex);
        }
    }
}
