using System.Linq;

namespace OpenSage.Network
{
    public class SkirmishGameSettings
    {
        public const int MaxNumberOfPlayers = 8;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _isDirty;
        private string _mapName;
        private SkirmishGameStatus _status;

        public SkirmishGameSettings(bool isHost)
        {
            IsHost = isHost;
            Slots = new SkirmishSlot[MaxNumberOfPlayers];
            for (var i = 0; i < Slots.Length; i++)
            {
                Slots[i] = new SkirmishSlot(i);
            }
            Status = SkirmishGameStatus.Configuring;
        }

        public bool IsHost { get; }

        public SkirmishGameStatus Status
        {
            get => _status;
            internal set
            {
                _status = value;
                Logger.Trace($"Skirmish game status is now {value}");
            }
        }

        public string MapName
        {
            get => _mapName;
            set
            {
                Logger.Trace($"MapName set to {value}");

                _mapName = value;
                IsDirty |= true;
            }
        }

        public bool IsDirty
        {
            get => _isDirty || Slots.Any(s => s.IsDirty);
            private set => _isDirty = value;
        }

        public void ResetDirty()
        {
            IsDirty = false;

            foreach (var slot in Slots)
            {
                slot.ResetDirty();
            }
        }

        public SkirmishSlot[] Slots { get; internal set; }
        public int LocalSlotIndex { get; set; } = -1;
        public SkirmishSlot LocalSlot { get { return (LocalSlotIndex < 0 || LocalSlotIndex >= Slots.Length) ? null : Slots[LocalSlotIndex]; } }
        public int Seed { get; internal set; }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            var unknown5 = reader.ReadUInt32(); // 25600 (160^2)
            var unknown6 = reader.ReadInt32();
            var unknown7 = reader.ReadBoolean();
            var unknown8 = reader.ReadBoolean();
            var unknown9 = reader.ReadBoolean();
            var unknown10 = reader.ReadUInt32(); // 0

            var numPlayers = reader.ReadUInt32(); // 8
            if (numPlayers != MaxNumberOfPlayers)
            {
                throw new InvalidStateException();
            }

            Slots = new SkirmishSlot[MaxNumberOfPlayers];
            for (var i = 0; i < Slots.Length; i++)
            {
                Slots[i] = new SkirmishSlot(i);
                Slots[i].Load(reader);
            }

            reader.SkipUnknownBytes(4);

            MapName = reader.ReadAsciiString();

            var mapFileCrc = reader.ReadUInt32();
            var mapFileSize = reader.ReadUInt32();

            var unknown12 = reader.ReadUInt32();
            var unknown13 = reader.ReadUInt32();
        }
    }
}
