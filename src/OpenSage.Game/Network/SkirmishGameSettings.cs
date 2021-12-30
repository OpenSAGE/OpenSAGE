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

        private uint _unknownInt1;
        private int _unknownInt2;
        private bool _unknownBool1;
        private bool _unknownBool2;
        private bool _unknownBool3;
        private uint _unknownInt3;
        private uint _mapFileCrc;
        private uint _mapFileSize;
        private uint _unknownInt4;
        private uint _unknownInt5;

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

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.PersistUInt32(ref _unknownInt1); // 25600 (160^2)
            reader.PersistInt32(ref _unknownInt2);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
            reader.PersistBoolean(ref _unknownBool3);
            reader.PersistUInt32(ref _unknownInt3); // 0

            var numPlayers = (uint)MaxNumberOfPlayers;
            reader.PersistUInt32(ref numPlayers); // 8
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

            reader.PersistAsciiString(ref _mapName);
            reader.PersistUInt32(ref _mapFileCrc);
            reader.PersistUInt32(ref _mapFileSize);
            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _unknownInt5);
        }
    }
}
