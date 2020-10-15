using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSage.Network.Packets
{
    public class SkirmishSlotStatusPacket
    {
        // TODO Map
        public SkirmishSlot[] Slots { get; set; }
    }
}
