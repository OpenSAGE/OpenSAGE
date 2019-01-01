using System;
using System.Collections.Generic;
using OpenSage.Logic.Orders;

namespace OpenSage.Network
{
    public sealed class EchoConnection : IConnection
    {
        private struct ReceivedPacket
        {
            public uint Frame;
            public List<Order> Orders;
        }

        private readonly List<ReceivedPacket> _receivedPackets = new List<ReceivedPacket>();

        public bool IsReplay { get; } = false;

        public void Send(uint frame, List<Order> orders)
        {
            _receivedPackets.Add(new ReceivedPacket
            {
                Frame = frame,
                Orders = orders
            });
        }

        public void Receive(uint frame, Action<uint, Order> packetFn)
        {
            foreach (var packet in _receivedPackets)
            {
                foreach (var order in packet.Orders)
                {
                    packetFn(packet.Frame, order);
                }
            }

            _receivedPackets.Clear();
        }

        public void Dispose()
        {
            
        }
    }
}
