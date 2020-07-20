using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
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

        public SkirmishManager(Game game)
        {
            _game = game;

            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener)
            {
                ReuseAddress = true,
                IPv6Enabled = false, // TODO: temporary
            };

            _writer = new NetDataWriter();

            _processor = new NetPacketProcessor();
            _processor.RegisterNestedType(SkirmishSlot.Serialize, SkirmishSlot.Deserialize);
            _processor.SubscribeReusable<SkirmishClientConnectPacket, SkirmishSlot>(SkirmishClientConnectPacketReceived);
            _processor.SubscribeReusable<SkirmishClientUpdatePacket, int>(SkirmishClientUpdatePacketReceived);
            _processor.SubscribeReusable<SkirmishStatusPacket>(SkirmishStatusPacketReceived);

            _listener.ConnectionRequestEvent += request =>
            {
                var nextFreeSlot = SkirmishGame.Slots.FirstOrDefault(s => s.State == SkirmishSlotState.Open);
                if (nextFreeSlot != null)
                {
                    var peer = request.Accept();

                    _processor.ReadPacket(request.Data, nextFreeSlot);

                    nextFreeSlot.State = SkirmishSlotState.Human;
                    nextFreeSlot.PeerId = peer.Id;
                }
                else
                {
                    request.Reject();
                }
            };         

            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var type = (PacketType) dataReader.GetByte();
                switch (type)
                {
                    case PacketType.SkirmishStatus:
                        _processor.ReadPacket(dataReader);
                        break;
                    case PacketType.SkirmishClientUpdate:
                        _processor.ReadPacket(dataReader, fromPeer.Id);
                        break;
                }
            };
        }

        private void SkirmishClientUpdatePacketReceived(SkirmishClientUpdatePacket packet, int peerId)
        {
            var slot = SkirmishGame.Slots.FirstOrDefault(s => s.PeerId == peerId);
            if (slot != null)
            {
                slot.PlayerName = packet.PlayerName;
            }
        }

        private void SkirmishStatusPacketReceived(SkirmishStatusPacket packet)
        {
            SkirmishGame.Slots = packet.Slots;
        }

        private void SkirmishClientConnectPacketReceived(SkirmishClientConnectPacket packet, SkirmishSlot slot)
        {
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

            SkirmishGame = new SkirmishGame(isHost: true);
            var localSlot = SkirmishGame.Slots[0];
            localSlot.PlayerName = _game.LobbyManager.Username;
            localSlot.State = SkirmishSlotState.Human;

            IsHosting = true;
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
            _processor.Write(_writer, new SkirmishClientConnectPacket() { PlayerName = _game.LobbyManager.Username });

            var server = _manager.Connect(endPoint.Address.ToString(), Ports.SkirmishHost, _writer);

            //_writer.Put((byte) PacketType.SkirmishClientUpdate);
            //_processor.Write(_writer, new SkirmishClientUpdatePacket() { PlayerName = _game.LobbyManager.Username });
            //server.Send(_writer, DeliveryMethod.ReliableOrdered);

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
                    _writer.Put((byte) PacketType.SkirmishStatus);
                    _processor.Write(_writer, new SkirmishStatusPacket()
                    {
                        Slots = SkirmishGame.Slots
                    });

                    _manager.SendToAll(_writer, DeliveryMethod.Unreliable);
                }

                Thread.Sleep(100);
            }
        }
    }
}
