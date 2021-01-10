using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    public sealed class ClientNetworkConnection : NetworkConnection
    {
        private IPAddress _hostAddress;

        public ClientNetworkConnection(SkirmishGame game): base(game)
        {
            _hostAddress = game.Slots[0].EndPoint.Address;
        }

        public override Task InitializeAsync()
        {
            if (!_manager.Start(Ports.AnyAvailable))
            {
                Logger.Error("Failed to initialize network connection");
            };

            var host = new IPEndPoint(_hostAddress, Ports.SkirmishGame);
            Logger.Trace($"Initializing network connection from port {_manager.LocalPort} to {host}");
            _manager.Connect(host, string.Empty);

            return Task.Run(async () =>
            {
                while (_manager.ConnectedPeersCount < 1)
                {
                    _manager.PollEvents();
                    await Task.Delay(10);
                }
            });
        }
    }

    public sealed class HostNetworkConnection : NetworkConnection
    {
        public HostNetworkConnection(SkirmishGame game) : base(game)
        {
            _listener.ConnectionRequestEvent += request =>
            {
                Logger.Trace($"Accepting connection from {request.RemoteEndPoint}");

                var peer = request.Accept();

                Logger.Trace($"Accept result: {peer}");
            };
        }

        public override Task InitializeAsync()
        {
            if (!_manager.Start(Ports.SkirmishGame))
            {
                Logger.Error("Failed to initialize network connection");
            };

            return Task.Run(async () =>
            {
                while (_manager.ConnectedPeersCount < _numberOfOtherPlayers)
                {
                    _manager.PollEvents();
                    await Task.Delay(10);
                }
            });
        }

        protected override void ProcessOrderPacket(SkirmishOrderPacket packet, NetPeer sender)
        {
            DistributePacketToOtherClients(packet, sender);
            base.ProcessOrderPacket(packet, sender);
        }

        private void DistributePacketToOtherClients(SkirmishOrderPacket packet, NetPeer sender)
        {
            _writer.Reset();
            _processor.Write(_writer, packet);
            foreach (var peer in _manager.ConnectedPeerList)
            {
                if (peer != sender)
                {
                    Logger.Trace($"  Redistributing to {peer.EndPoint}");
                    peer.Send(_writer, DeliveryMethod.ReliableUnordered);
                }
            }
        }
    }

    public abstract class NetworkConnection : EchoConnection
    {
        protected const int OrderSchedulingOffsetInFrames = 2;

        protected EventBasedNetListener _listener;
        protected NetManager _manager;
        protected NetPacketProcessor _processor;
        protected NetDataWriter _writer;
        protected Dictionary<uint, int> _receivedPacketsPerFrame = new Dictionary<uint, int>();

        protected int _numberOfOtherPlayers;

        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public NetworkConnection(SkirmishGame game)
        {
            _numberOfOtherPlayers = game.Slots.Count(s => s.State == SkirmishSlotState.Human) - 1;
            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener);

            if (Debugger.IsAttached)
            {
                _manager.DisconnectTimeout = 600000;
            }

            _listener.PeerConnectedEvent += peer => Logger.Trace($"{peer.EndPoint} connected");
            _listener.PeerDisconnectedEvent += (peer, info) => Logger.Trace($"{peer.EndPoint} disconnected with reason {info.Reason}");
            _listener.NetworkReceiveEvent += (NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) => _processor.ReadAllPackets(reader, peer);

            _writer = new NetDataWriter();
            _processor = new NetPacketProcessor();            
            _processor.RegisterNestedType<Order>(WriteOrder, ReadOrder);
            _processor.Subscribe<SkirmishOrderPacket, NetPeer>(ReceiveOrderPacket, () => new SkirmishOrderPacket());
        }

        public abstract Task InitializeAsync();

        private void ReceiveOrderPacket(SkirmishOrderPacket packet, NetPeer sender)
        {
            Logger.Trace($"Received packet for frame {packet.Frame} from {sender.EndPoint}");
            ProcessOrderPacket(packet, sender);
        }

        protected virtual void ProcessOrderPacket(SkirmishOrderPacket packet, NetPeer sender)
        {
            _receivedPacketsPerFrame.TryGetValue(packet.Frame, out int count);
            _receivedPacketsPerFrame[packet.Frame] = count + 1;

            Logger.Trace($"  Packet count for frame {packet.Frame} is now {count + 1}");

            StorePacket(packet);
        }

        public override void Receive(uint frame, Action<uint, Order> packetFn)
        {
            _manager.PollEvents();

            if (frame < OrderSchedulingOffsetInFrames)
            {
                return;
            }

            while (!_receivedPacketsPerFrame.TryGetValue(frame, out var count) || count < _numberOfOtherPlayers)
            {
                _manager.PollEvents();
                Thread.Sleep(1);
            }

            base.Receive(frame, packetFn);

            _receivedPacketsPerFrame.Remove(frame);
        }

        public override void Send(uint frame, List<Order> orders)
        {
            var scheduledFrame = frame + OrderSchedulingOffsetInFrames;
            Logger.Trace($"Frame {frame}: Sending {orders.Count} for frame {scheduledFrame}");

            var packet = new SkirmishOrderPacket()
            {
                Frame = scheduledFrame,
                Orders = orders
            };

            StorePacket(packet);

            _writer.Reset();
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
