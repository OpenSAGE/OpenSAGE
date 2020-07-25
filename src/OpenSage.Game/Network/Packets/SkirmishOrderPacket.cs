using System.Collections.Generic;
using OpenSage.Logic.Orders;

namespace OpenSage.Network.Packets
{
    public class SkirmishOrderPacket
    {
        public uint Frame { get; set; }
        public Order[] Orders { get; set; }
    }
}
