using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Network
{
    public class SkirmishGame
    {
        public SkirmishGame(bool isHost)
        {
            this.IsHost = isHost;
            this.Slots = Enumerable.Range(0, 8).Select(i => new SkirmishSlot(i)).ToArray();
        }

        public bool IsHost { get; }

        public SkirmishSlot[] Slots { get; set; }
        public int LocalSlotIndex { get; set; } = -1;
        public SkirmishSlot? LocalSlot { get { return Slots[LocalSlotIndex]; } }
        public bool ReadyToStart { get; internal set; }
    }
}
