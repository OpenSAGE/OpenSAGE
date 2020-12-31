using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    public sealed class NetworkConnection : EchoConnection
    {
        private const int OrderSchedulingOffsetInFrames = 2;

        private EventBasedNetListener _listener;
        private NetManager _manager;
        private NetPacketProcessor _processor;
        private NetDataWriter _writer;
        private Dictionary<uint, int> _receivedPacketsPerFrame = new Dictionary<uint, int>();

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public NetworkConnection()
        {
            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener)
            {
                ReuseAddress = true,
                IPv6Enabled = IPv6Mode.Disabled, // TODO: temporary
            };

            if (Debugger.IsAttached)
            {
                _manager.DisconnectTimeout = 600000;
            }

            _listener.PeerConnectedEvent += peer => Logger.Trace($"{peer.EndPoint} connected");
            _listener.PeerDisconnectedEvent += (peer, info) => Logger.Trace($"{peer.EndPoint} disconnected with reason {info.Reason}");

            _listener.ConnectionRequestEvent += request =>
            {
                Logger.Trace($"Accepting connection from {request.RemoteEndPoint}");

                var peer = request.Accept();

                Logger.Trace($"Accept result: {peer}");
            };

            _listener.NetworkReceiveEvent += (NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) => _processor.ReadAllPackets(reader);

            _writer = new NetDataWriter();
            _processor = new NetPacketProcessor();            
            _processor.RegisterNestedType<Order>(WriteOrder, ReadOrder);
            _processor.Subscribe<SkirmishOrderPacket>(packet =>
            {
                if (_receivedPacketsPerFrame.TryGetValue(packet.Frame, out int count))
                {
                    count++;
                }
                else
                {
                    _receivedPacketsPerFrame.Add(packet.Frame, 1);
                }

                StorePacket(packet);
            },
            () => new SkirmishOrderPacket());
        }

        public Task InitializeAsync(Game game)
        {
            _manager.Start(IPAddress.Local, System.Net.IPAddress.IPv6Any, Ports.AnyAvailable); // TODO: what about IPV6
            _listener.PeerConnectedEvent += peer => Logger.Trace($"Connected to {peer.EndPoint}"); ;

            foreach (var slot in game.SkirmishManager.SkirmishGame.Slots)
            {
                if (slot.State == SkirmishSlotState.Human && slot.Index > game.SkirmishManager.SkirmishGame.LocalSlotIndex)
                {
                    Logger.Trace($"Connecting to {slot.EndPoint}");
                    _manager.Connect(slot.EndPoint, string.Empty);
                }
            }

            return Task.Run(async () =>
            {
                var humanPlayers = game.SkirmishManager.SkirmishGame.Slots.Count(s => s.State == SkirmishSlotState.Human);
                while (_manager.ConnectedPeersCount < humanPlayers - 1)
                {
                    _manager.PollEvents();
                    await Task.Delay(10);
                }
            });
        }

        public override void Receive(uint frame, Action<uint, Order> packetFn)
        {
            _manager.PollEvents();

            if (frame < OrderSchedulingOffsetInFrames)
            {
                return;
            }

            while (!_receivedPacketsPerFrame.TryGetValue(frame, out var count) || count < _manager.ConnectedPeersCount)
            {
                _manager.PollEvents();
            }

            base.Receive(frame, packetFn);

            _receivedPacketsPerFrame.Remove(frame);
        }

        public override void Send(uint frame, List<Order> orders)
        {
            var scheduledFrame = frame + OrderSchedulingOffsetInFrames;

            base.Send(scheduledFrame, orders);

            _writer.Reset();
            var packet = new SkirmishOrderPacket()
            {
                Frame = scheduledFrame,
                Orders = orders
            };

            _processor.Write(_writer, packet);
            _manager.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
        }

        private Order ReadOrder(NetDataReader reader)
        {
            int playerIndex = reader.GetInt();
            OrderType orderType = (OrderType) reader.GetInt();

            var order = new Order(playerIndex, orderType);

            byte argumentCount = reader.GetByte();

            for (int i = 0; i < argumentCount; i++)
            {
                OrderArgumentType argumentType = (OrderArgumentType)reader.GetInt();

                switch (argumentType)
                {
                    case OrderArgumentType.Integer:
                        order.AddIntegerArgument(reader.GetInt());
                        break;
                    case OrderArgumentType.Float:
                        order.AddFloatArgument(reader.GetFloat());
                        break;
                    case OrderArgumentType.Boolean:
                        order.AddBooleanArgument(reader.GetBool());
                        break;
                    case OrderArgumentType.ObjectId:
                        order.AddObjectIdArgument(reader.GetUInt());
                        break;
                    case OrderArgumentType.Position:
                        order.AddPositionArgument(new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat()));
                        break;
                    case OrderArgumentType.ScreenPosition:
                        order.AddScreenPositionArgument(new Point2D(reader.GetInt(), reader.GetInt()));
                        break;
                    case OrderArgumentType.ScreenRectangle:
                        order.AddScreenRectangleArgument(new Rectangle(reader.GetInt(), reader.GetInt(),
                                                                       reader.GetInt(), reader.GetInt()));
                        break;
                    default:
                        throw new NotImplementedException("We don't know the other argument types");
                }
            }

            return order;
        }

        private void WriteOrder(NetDataWriter writer, Order order)
        {
            writer.Put(order.PlayerIndex);
            writer.Put((int) order.OrderType);
            writer.Put((byte) order.Arguments.Count);

            foreach (var argument in order.Arguments)
            {
                writer.Put((int) argument.ArgumentType);

                switch (argument.ArgumentType)
                {
                    case OrderArgumentType.Integer:
                        writer.Put(argument.Value.Integer);
                        break;
                    case OrderArgumentType.Float:
                        writer.Put(argument.Value.Float);
                        break;
                    case OrderArgumentType.Boolean:
                        writer.Put(argument.Value.Boolean);
                        break;
                    case OrderArgumentType.ObjectId:
                        writer.Put(argument.Value.ObjectId);
                        break;
                    case OrderArgumentType.Position:
                        writer.Put(argument.Value.Position.X);
                        writer.Put(argument.Value.Position.Y);
                        writer.Put(argument.Value.Position.Z);
                        break;
                    case OrderArgumentType.ScreenPosition:
                        writer.Put(argument.Value.ScreenPosition.X);
                        writer.Put(argument.Value.ScreenPosition.Y);
                        break;
                    case OrderArgumentType.ScreenRectangle:
                        writer.Put(argument.Value.ScreenRectangle.X);
                        writer.Put(argument.Value.ScreenRectangle.Y);
                        writer.Put(argument.Value.ScreenRectangle.Width);
                        writer.Put(argument.Value.ScreenRectangle.Height);
                        break;
                    default:
                        throw new NotImplementedException("We don't know the other argument types");
                }
            }
        }

        public override void Dispose()
        {
            _manager.Stop();
        }
    }
}
