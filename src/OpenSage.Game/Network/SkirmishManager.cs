using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    public abstract class SkirmishManager
    {
        private string _connectionKey = string.Empty; // TODO: maybe use this for password protection?

        protected Game _game;
        protected NetPacketProcessor _processor;
        protected NetDataWriter _writer;
        protected EventBasedNetListener _listener;
        protected NetManager _manager;

        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public bool IsHosting { get; private set; }

        public IConnection Connection { get; private set; }

        protected void ThreadProc()
        {
            while (_isRunning)
            {
                _manager.PollEvents();

                _writer.Reset();

                Loop();

                try
                {
                    Thread.Sleep(100);
                }
                catch (ThreadInterruptedException)
                {
                    //ignore it, should be stopped by _isRunning
                }
            }
        }

        protected abstract void Loop();

        protected SkirmishManager(Game game, bool isHosting)
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

            IsHosting = isHosting;

            SkirmishGame = new SkirmishGame(isHost: IsHosting);

        }

        public SkirmishGame SkirmishGame { get; private set; }
        public bool IsReady
        {
            get
            {
                //no unready human players, except for the host
                return SkirmishGame.Slots.Where(x => x.Index != 0 && x.State == SkirmishSlotState.Human && !x.Ready).Count() == 0;
            }
        }

        public void StartGame()
        {
            _manager.SendToAll(new[] { (byte) PacketType.SkirmishStartGame }, DeliveryMethod.ReliableUnordered);

            CreateNetworkConnection();
        }

        public void Quit()
        {
            _manager.DisconnectAll();
            _manager.Stop();

            Stop();
        }

        private void Stop()
        {
            _isRunning = false;
            _thread?.Interrupt();
            _thread?.Join();
            _thread = null;

            if (UPnP.Status == UPnPStatus.PortsForwarded)
            {
                UPnP.RemovePortForwardingAsync().Wait();
            }
        }

        protected Thread _thread;
        protected bool _isRunning;

        protected void Start()
        {
            _isRunning = true;
            _thread = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = "OpenSAGE Skirmish Manager"
            };
            _thread.Start();
        }

        private async void CreateNetworkConnection()
        {
            var connection = new NetworkConnection();
            await connection.InitializeAsync(_game);

            Connection = connection;

            SkirmishGame.ReadyToStart = true;
        }

        public class Client : SkirmishManager
        {
            private string _playerId;

            public Client(Game game, IPEndPoint endPoint) : base(game, false)
            {
                _processor.SubscribeReusable<SkirmishSlotStatusPacket>(SkirmishStatusPacketReceived);

                _manager.Start(IPAddress.Local, System.Net.IPAddress.IPv6Any, Ports.AnyAvailable); // TODO: what about IPV6

                _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
                {
                    var type = (PacketType) dataReader.GetByte();
                    Logger.Info($"Have packet with type {type}");
                    switch (type)
                    {
                        case PacketType.SkirmishSlotStatus:
                            _processor.ReadPacket(dataReader);
                            break;

                        case PacketType.SkirmishClientUpdate:
                            Logger.Error($"Client should never receive client update");
                            break;

                        case PacketType.SkirmishStartGame:
                            Logger.Trace($"Received start game packet");
                            CreateNetworkConnection();
                            break;
                    }
                };

                Logger.Trace($"Joining game at {endPoint}");

                _playerId = Guid.NewGuid().ToString();

                _writer.Reset();
                _processor.Write(_writer, new SkirmishClientConnectPacket()
                {
                    PlayerName = _game.LobbyManager.Username,
                    PlayerId = _playerId,
                    ProcessId = Process.GetCurrentProcess().Id
                }); ; ;

                _manager.Connect(endPoint.Address.ToString(), Ports.SkirmishHost, _writer);

                Start();
            }

            private void SkirmishStatusPacketReceived(SkirmishSlotStatusPacket packet)
            {
                Logger.Info($"New skirmish slot status");
                SkirmishGame.Slots = packet.Slots;
                if (SkirmishGame.LocalSlotIndex < 0)
                {
                    SkirmishGame.LocalSlotIndex = Array.FindIndex(packet.Slots, s => s.PlayerId == _playerId);
                    Logger.Info($"New Local slot index is {SkirmishGame.LocalSlotIndex}");
                }
            }

            protected override void Loop()
            {
                var localSlot = SkirmishGame.LocalSlot;
                if (localSlot != null && localSlot.IsDirty)
                {
                    Logger.Info($"Local slot is dirty, sending...");
                    _writer.Put((byte) PacketType.SkirmishClientUpdate);
                    _processor.Write(_writer, new SkirmishClientUpdatePacket()
                    {
                        Ready = localSlot.Ready,
                        PlayerName = localSlot.PlayerName,
                        ColorIndex = localSlot.ColorIndex,
                        FactionIndex = localSlot.FactionIndex,
                        Team = localSlot.Team,
                    });

                    _manager.SendToAll(_writer, DeliveryMethod.ReliableUnordered);
                    localSlot.ResetDirty();
                }
            }
        }

        public class Host : SkirmishManager
        {

            private Dictionary<int, SkirmishSlot> _slotLookup = new Dictionary<int, SkirmishSlot>();

            private void SkirmishClientUpdatePacketReceived(SkirmishClientUpdatePacket packet, SkirmishSlot slot)
            {
                if (slot != null)
                {
                    slot.PlayerName = packet.PlayerName;
                    slot.Ready = packet.Ready;
                    slot.Team = packet.Team;
                    slot.FactionIndex = packet.FactionIndex;
                    slot.ColorIndex = packet.ColorIndex;
                }
            }

            private void SkirmishClientConnectPacketReceived(SkirmishClientConnectPacket packet, SkirmishSlot slot)
            {
                slot.PlayerName = packet.PlayerName;
                slot.PlayerId = packet.PlayerId;
            }

            public Host(Game game) : base(game, true)
            {
                _processor.SubscribeReusable<SkirmishClientConnectPacket, SkirmishSlot>(SkirmishClientConnectPacketReceived);
                _processor.SubscribeReusable<SkirmishClientUpdatePacket, SkirmishSlot>(SkirmishClientUpdatePacketReceived);

                _listener.PeerConnectedEvent += peer => Logger.Trace($"{peer.EndPoint} connected");

                _listener.PeerDisconnectedEvent += (peer, info) =>
                {
                    Logger.Trace($"{peer.EndPoint} disconnected with reason {info.Reason}");

                    var slot = _slotLookup[peer.Id];
                    slot.State = SkirmishSlotState.Open;
                    slot.PlayerId = null;
                    slot.PlayerName = null;
                    slot.Ready = false;

                    _slotLookup.Remove(peer.Id);
                };

                _listener.ConnectionRequestEvent += request =>
                {
                    var nextFreeSlot = SkirmishGame.Slots.FirstOrDefault(s => s.State == SkirmishSlotState.Open);
                    if (nextFreeSlot != null)
                    {
                        Logger.Trace($"Accepting connection from {request.RemoteEndPoint}");

                        Logger.Info($"Have Data: {request.Data.AvailableBytes}");

                        var peer = request.Accept();

                        _processor.ReadPacket(request.Data, nextFreeSlot);

                        nextFreeSlot.State = SkirmishSlotState.Human;
                        nextFreeSlot.EndPoint = peer.EndPoint;
                        _slotLookup.Add(peer.Id, nextFreeSlot);
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
                    var slot = _slotLookup[fromPeer.Id];
                    switch (type)
                    {
                        case PacketType.SkirmishSlotStatus:
                            Logger.Error($"Host should never receive slot status updates");
                            break;
                        case PacketType.SkirmishClientUpdate:
                            _processor.ReadPacket(dataReader, slot);
                            break;
                        case PacketType.SkirmishStartGame:
                            Logger.Trace($"Received start game packet");
                            Debug.Assert(false, "Host should never receive SkirmishStartGame");
                            CreateNetworkConnection();
                            break;
                    }
                };


                _manager.Start(IPAddress.Local, System.Net.IPAddress.IPv6Any, Ports.SkirmishHost); // TODO: what about IPV6
                
                SkirmishGame.LocalSlotIndex = 0;

                var localSlot = SkirmishGame.LocalSlot;
                localSlot.PlayerName = _game.LobbyManager.Username;
                localSlot.State = SkirmishSlotState.Human;
                localSlot.EndPoint = NetUtils.MakeEndPoint(NetUtils.GetLocalIp(LocalAddrType.IPv4), Ports.SkirmishHost);

                Start();
            }

            protected override void Loop()
            {
                var dirty = SkirmishGame.Slots.Where(x => x.IsDirty);
                if (dirty.Count() > 0)
                {
                    Logger.Info($"Have {dirty.Count()} dirty slots");

                    _writer.Put((byte) PacketType.SkirmishSlotStatus);
                    _processor.Write(_writer, new SkirmishSlotStatusPacket()
                    {
                        Slots = SkirmishGame.Slots
                    });

                    _manager.SendToAll(_writer, DeliveryMethod.Unreliable);

                    foreach (var slot in dirty)
                    {
                        slot.ResetDirty();
                    }
                }
            }
        }
    }
}
