using System.Linq;

namespace OpenSage.Network
{
    public class SkirmishGame
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _isDirty;
        private string _mapName;
        private SkirmishGameStatus _status;

        public SkirmishGame(bool isHost)
        {
            IsHost = isHost;
            Slots = Enumerable.Range(0, 8).Select(i => new SkirmishSlot(i)).ToArray();
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
        public bool ReadyToStart { get; internal set; }
        public int Seed { get; internal set; }
    }
}
