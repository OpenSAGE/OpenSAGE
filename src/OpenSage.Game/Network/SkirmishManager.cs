using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenSage.Content;
using OpenSage.Logic;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    public class SkirmishManager
    {
        private string _connectionKey = string.Empty; // TODO: maybe use this for password protection?

        private Game _game;
        private EventBasedNetListener _listener;
        private NetManager _manager;
        private Thread _thread;
        private bool _isRunning;
        private NetPacketProcessor _processor;
        private NetDataWriter _writer;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public bool IsHosting { get; set; }

        public IConnection Connection { get; private set; }

        public SkirmishManager(Game game)
        {
            _game = game;

            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener)
            {
                ReuseAddress = true,
                IPv6Enabled = IPv6Mode.Disabled, // TODO: temporary
            };

            _writer = new NetDataWriter();

            _processor = new NetPacketProcessor();
            _processor.RegisterNestedType(SkirmishSlot.Serialize, SkirmishSlot.Deserialize);
            _processor.SubscribeReusable<SkirmishClientConnectPacket, SkirmishSlot>(SkirmishClientConnectPacketReceived);
            _processor.SubscribeReusable<SkirmishClientUpdatePacket, (IPEndPoint endPoint, int processId)>(SkirmishClientUpdatePacketReceived);
            _processor.SubscribeReusable<SkirmishSlotStatusPacket>(SkirmishStatusPacketReceived);

            _listener.PeerConnectedEvent += peer => Logger.Trace($"{peer.EndPoint} connected");
            _listener.PeerDisconnectedEvent += (peer, info) => Logger.Trace($"{peer.EndPoint} disconnected with reason {info.Reason}");

            _listener.ConnectionRequestEvent += request =>
            {
                var nextFreeSlot = SkirmishGame.Slots.FirstOrDefault(s => s.State == SkirmishSlotState.Open);
                if (nextFreeSlot != null)
                {
                    Logger.Trace($"Accepting connection from {request.RemoteEndPoint}");

                    var peer = request.Accept();

                    _processor.ReadPacket(request.Data, nextFreeSlot);

                    nextFreeSlot.State = SkirmishSlotState.Human;
                    nextFreeSlot.EndPoint = peer.EndPoint;
                }
                else
                {
                    Logger.Trace($"Rejecting connection from {request.RemoteEndPoint}");
                    request.Reject();
                }
            };         

            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var type = (PacketType) dataReader.GetByte();
                switch (type)
                {
                    case PacketType.SkirmishSlotStatus:
                        _processor.ReadPacket(dataReader);
                        break;
                    case PacketType.SkirmishClientUpdate:
                        _processor.ReadPacket(dataReader, fromPeer.Id);
                        break;
                    case PacketType.SkirmishStartGame:
                        Logger.Trace($"Received start game packet");

                        CreateNetworkConnection();
                        break;
                }
            };
        }

        private void SkirmishClientUpdatePacketReceived(SkirmishClientUpdatePacket packet, (IPEndPoint endPoint, int processId) peer)
        {
            var slot = SkirmishGame.Slots.FirstOrDefault(s => s.EndPoint.Equals(peer.endPoint) && s.ProcessId == peer.processId);
            if (slot != null)
            {
                slot.PlayerName = packet.PlayerName;
            }
        }

        private void SkirmishStatusPacketReceived(SkirmishSlotStatusPacket packet)
        {
            SkirmishGame.Slots = packet.Slots;
            if (SkirmishGame.LocalSlotIndex < 0)
            {
                for (int i = 0; i < packet.Slots.Length; i++)
                {
                    if (packet.Slots[i].EndPoint.Address.ToString() == NetUtils.GetLocalIp(LocalAddrType.IPv4) &&
                        packet.Slots[i].ProcessId == Process.GetCurrentProcess().Id)
                    {
                        SkirmishGame.LocalSlotIndex = i;
                        break;
                    }
                }
            }
        }

        private void SkirmishClientConnectPacketReceived(SkirmishClientConnectPacket packet, SkirmishSlot slot)
        {
            slot.ProcessId = packet.ProcessId;
            slot.PlayerName = packet.PlayerName;
        }

        public SkirmishGame SkirmishGame { get; private set; }

        public void HostGame()
        {
            if (_game.Configuration.LanIpAddress != IPAddress.Any)
            {
                Logger.Trace($"Starting network manager using configured IP Address { _game.Configuration.LanIpAddress }");
                _manager.Start(_game.Configuration.LanIpAddress, IPAddress.IPv6Any, Ports.SkirmishHost); // TODO: what about IPV6
            }
            else
            {
                Logger.Trace($"Starting network manager using default IP Address.");
                _manager.Start(Ports.SkirmishHost);
            }

            _isRunning = true;
            _thread = new Thread(Loop)
            {
                IsBackground = true,
                Name = "OpenSAGE Skirmish Manager"
            };
            _thread.Start();

            SkirmishGame = new SkirmishGame(isHost: true)
            {
                LocalSlotIndex = 0
            };

            var localSlot = SkirmishGame.Slots[SkirmishGame.LocalSlotIndex];
            localSlot.PlayerName = _game.LobbyManager.Username;
            localSlot.State = SkirmishSlotState.Human;
            localSlot.EndPoint = NetUtils.MakeEndPoint(NetUtils.GetLocalIp(LocalAddrType.IPv4), Ports.SkirmishHost);

            IsHosting = true;
        }

        public void StartGame()
        {
            _manager.SendToAll(new[] { (byte) PacketType.SkirmishStartGame }, DeliveryMethod.ReliableUnordered);

            CreateNetworkConnection();
        }

        public void JoinGame(IPEndPoint endPoint)
        {
            if (_game.Configuration.LanIpAddress != IPAddress.Any)
            {
                Logger.Trace($"Starting network manager using configured IP Address { _game.Configuration.LanIpAddress }");
                _manager.Start(_game.Configuration.LanIpAddress, IPAddress.IPv6Any, Ports.SkirmishClient); // TODO: what about IPV6
            }
            else
            {
                Logger.Trace($"Starting network manager using default IP Address.");
                _manager.Start(Ports.SkirmishClient);
            }

            Logger.Trace($"Joining game at {endPoint}");

            _writer.Reset();
            _processor.Write(_writer, new SkirmishClientConnectPacket()
            {
                PlayerName = _game.LobbyManager.Username,
                ProcessId = Process.GetCurrentProcess().Id
            });

            _manager.Connect(endPoint.Address.ToString(), Ports.SkirmishHost, _writer);

            SkirmishGame = new SkirmishGame(isHost: false);

            _isRunning = true;
            _thread = new Thread(Loop)
            {
                IsBackground = true,
                Name = "OpenSAGE Skirmish Manager"
            };
            _thread.Start();
        }

        public void Quit()
        {
            IsHosting = false;

            _manager.Stop();

            _isRunning = false;
            _thread?.Join();
            _thread = null;
        }

        private void Loop()
        {
            while (_isRunning)
            {
                _manager.PollEvents();

                _writer.Reset();

                if (IsHosting)
                {
                    _writer.Put((byte) PacketType.SkirmishSlotStatus);
                    _processor.Write(_writer, new SkirmishSlotStatusPacket()
                    {
                        Slots = SkirmishGame.Slots
                    });

                    _manager.SendToAll(_writer, DeliveryMethod.Unreliable);
                }

                Thread.Sleep(100);
            }
        }

        private async void CreateNetworkConnection()
        {
            var connection = new NetworkConnection();
            await connection.InitializeAsync(_game);

            Connection = connection;

            SkirmishGame.ReadyToStart = true;
        }
    }
}
