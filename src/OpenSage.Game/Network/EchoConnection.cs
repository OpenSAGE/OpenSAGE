using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using OpenSage.Logic.Orders;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    public class EchoConnection : IConnection
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly List<SkirmishOrderPacket> _receivedPackets = new List<SkirmishOrderPacket>();

        public virtual void Send(uint frame, List<Order> orders)
        {
            StorePacket(new SkirmishOrderPacket
            {
                Frame = frame,
                Orders = orders.ToArray()
            });
        }

        protected void StorePacket(SkirmishOrderPacket packet)
        {
            _receivedPackets.Add(packet);

            if (packet.Orders.Length > 0)
            {
                Logger.Trace($"Storing packet for frame {packet.Frame} with {packet.Orders.Length} orders, count is {_receivedPackets.Count}");
            }
        }

        public virtual void Receive(uint frame, Action<uint, Order> packetFn)
        {
            var notEmptyCount = _receivedPackets.Count(p => p.Orders.Any());
            if (notEmptyCount > 0)
            {
                Logger.Trace($"Processing { notEmptyCount } received packets in frame {frame}");
            }

            foreach (var packet in _receivedPackets)
            {
                if (packet.Orders.Length > 0)
                {
                    Logger.Trace($"  Processing received packet scheduled for frame {packet.Frame} in frame {frame}");
                }

                foreach (var order in packet.Orders)
                {
                    Logger.Trace($"    Invoking callback for order {order.OrderType}");
                    packetFn(packet.Frame, order);
                }
            }

            _receivedPackets.Clear();
        }

        public virtual void Dispose()
        {
            
        }
    }
}
