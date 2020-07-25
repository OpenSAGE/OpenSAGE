using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenSage.Logic.Orders;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    class NetworkConnection : IConnection
    {
        private EventBasedNetListener _listener;
        private NetManager _manager;
        private NetPacketProcessor _processor;
        private NetDataWriter _writer;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public NetworkConnection()
        {
            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener)
            {
                ReuseAddress = true,
                IPv6Enabled = false, // TODO: temporary
            };

            _listener.PeerConnectedEvent += peer => Logger.Trace($"{peer.EndPoint} connected");
            _listener.PeerDisconnectedEvent += (peer, info) => Logger.Trace($"{peer.EndPoint} disconnected with reason {info.Reason}");

            _listener.ConnectionRequestEvent += request =>
            {
                Logger.Trace($"Accepting connection from {request.RemoteEndPoint} ({request.Type})");

                var peer = request.Accept();

                Logger.Trace($"Accept result: {peer}");
            };

            _writer = new NetDataWriter();
            _processor = new NetPacketProcessor();
            _processor.RegisterNestedType<Order>(WriteOrder, ReadOrder);
            _processor.SubscribeReusable<SkirmishOrderPacket, Action<uint, Order>>((packet, packetFn) =>
            {
                foreach (var order in packet.Orders)
                {
                    Logger.Trace($"Received order: {order.OrderType}");
                    packetFn(packet.Frame, order);
                }
            });
        }

        public Task InitializeAsync(Game game)
        {
            // TODO the same code is used in SkirmishManager
            if (game.Configuration.LanIpAddress != IPAddress.Any)
            {
                Logger.Trace($"Initializing network connection using configured IP Address { game.Configuration.LanIpAddress }");
                _manager.Start(game.Configuration.LanIpAddress, IPAddress.IPv6Any, Ports.SkirmishGame + game.SkirmishManager.SkirmishGame.LocalSlot); // TODO: what about IPV6
            }
            else
            {
                Logger.Trace($"Initializing network connection using default IP Address: {Ports.SkirmishGame + game.SkirmishManager.SkirmishGame.LocalSlot}.");
                _manager.Start(Ports.SkirmishGame + game.SkirmishManager.SkirmishGame.LocalSlot);
            }

            _listener.PeerConnectedEvent += peer => Logger.Trace($"Connected to {peer.EndPoint}"); ;

            foreach (var slot in game.SkirmishManager.SkirmishGame.Slots)
            {                
                if (slot.State == SkirmishSlotState.Human && slot.Index > game.SkirmishManager.SkirmishGame.LocalSlot)
                {
                    Logger.Trace($"Connecting to {slot.EndPoint.Address}:{Ports.SkirmishGame + slot.Index}");
                    _manager.Connect(slot.EndPoint.Address.ToString(), Ports.SkirmishGame + slot.Index, string.Empty);
                }
            }

            return Task.Run(async () =>
            {
                while (_manager.ConnectedPeersCount < game.SkirmishManager.SkirmishGame.Slots.Count(s => s.State == SkirmishSlotState.Human) - 1)
                {
                    _manager.PollEvents();
                    await Task.Delay(10);
                }
            });
        }

        public void Receive(uint frame, Action<uint, Order> packetFn)
        {
            _listener.NetworkReceiveEvent += ReceiveEvent;
            _manager.PollEvents();
            _listener.NetworkReceiveEvent -= ReceiveEvent;

            void ReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                _processor.ReadAllPackets(reader, packetFn);
            }
        }

        public void Send(uint frame, List<Order> orders)
        {
            _writer.Reset();
            var packet = new SkirmishOrderPacket()
            {
                Frame = frame,
                Orders = orders.ToArray() // TODO optimize
            };

            _processor.Write(_writer, packet);
            _manager.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
        }

        private Order ReadOrder(NetDataReader reader)
        {
            return new Order(reader.GetInt(), (OrderType)reader.GetInt());
        }

        private void WriteOrder(NetDataWriter writer, Order order)
        {
            writer.Put(order.PlayerIndex);
            writer.Put((int)order.OrderType);
        }

        public void Dispose()
        {
            _manager.Stop();
        }
    }
}
