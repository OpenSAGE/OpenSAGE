using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    public abstract class SkirmishManager
    {
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected Game _game;

        public SkirmishManager(Game game, bool isHosting)
        {
            _game = game;

            IsHosting = isHosting;
            Settings = new SkirmishGameSettings(isHosting);
        }

        public bool IsHosting { get; protected set; }
        public SkirmishGameSettings Settings { get; protected set; }
        public IConnection Connection { get; protected set; }

        public abstract bool IsStartButtonEnabled();
        public abstract Task HandleStartButtonClickAsync();

        public virtual void Stop()
        {
        }
    }

    public sealed class LocalSkirmishManager : SkirmishManager
    {
        public LocalSkirmishManager(Game game)
            : base(game, isHosting: true)
        {
            Settings.LocalSlotIndex = 0;
            Settings.LocalSlot.State = SkirmishSlotState.Human;
        }

        public override bool IsStartButtonEnabled() => true;

        public override Task HandleStartButtonClickAsync()
        {
            Connection = new EchoConnection();
            Settings.Status = SkirmishGameStatus.ReadyToStart;
            return Task.CompletedTask;
        }
    }

    public abstract class NetworkSkirmishManager : SkirmishManager
    {
        private string _connectionKey = string.Empty; // TODO: maybe use this for password protection?

        protected Thread _thread;
        protected bool _isRunning;

        protected NetPacketProcessor _processor;
        protected NetDataWriter _writer;
        protected EventBasedNetListener _listener;
        protected NetManager _manager;

        protected void ThreadProc()
        {
            while (_isRunning)
            {
                _manager.PollEvents();

                _writer.Reset();

                Loop();

                Thread.Sleep(100);
            }

            _manager.DisconnectAll();
            _manager.Stop();
        }

        protected abstract void Loop();

        protected NetworkSkirmishManager(Game game, bool isHosting)
            : base(game, isHosting)
        {
            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener);

            _writer = new NetDataWriter();

            _processor = new NetPacketProcessor();
            _processor.RegisterNestedType(SkirmishSlot.Serialize, SkirmishSlot.Deserialize);
        }

        public override void Stop()
        {
            _isRunning = false;
            _thread = null;
        }

        protected void StartThread()
        {
            _isRunning = true;
            _thread = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = "OpenSAGE Skirmish Manager"
            };
            _thread.Start();
        }

        protected abstract Task CreateNetworkConnectionAsync();
    }

    public sealed class ClientSkirmishManager : NetworkSkirmishManager
    {
        private void SkirmishGameStatusPacketReceived(SkirmishGameStatusPacket packet, IPEndPoint host)
        {
            Logger.Info("got mapName" + packet.MapName);

            // the host may not know its external IP, but we know it
            packet.Slots[0].EndPoint = host;

            Settings.MapName = packet.MapName;
            Settings.Slots = packet.Slots;

            // after joining a game, we don't know our slot index, but once
            // we got the slot data from the host, we can figure it out
            if (Settings.LocalSlotIndex < 0)
            {
                Settings.LocalSlotIndex = Array.FindIndex(packet.Slots, s => s.ClientId == ClientInstance.Id);
                Logger.Info($"New local slot index is {Settings.LocalSlotIndex}");
            }

            foreach (var slot in Settings.Slots)
            {
                slot.Ready = false;
            }
        }

        private async void SkirmishStartGamePacketReceived(SkirmishStartGamePacket packet)
        {
            Settings.Seed = packet.Seed;
            Settings.Status = SkirmishGameStatus.WaitingForAllPlayers;

            await CreateNetworkConnectionAsync();

            Settings.Status = SkirmishGameStatus.ReadyToStart;
        }

        private void SkirmishClientReadyPacketReceived(SkirmishClientReadyPacket packet)
        {
            Settings.Slots[packet.Index].Ready = true;
        }

        public ClientSkirmishManager(Game game, IPEndPoint endPoint) : base(game, isHosting: false)
        {
            _processor.SubscribeReusable<SkirmishGameStatusPacket, IPEndPoint>(SkirmishGameStatusPacketReceived);
            _processor.SubscribeReusable<SkirmishStartGamePacket>(SkirmishStartGamePacketReceived);
            _processor.SubscribeReusable<SkirmishClientReadyPacket>(SkirmishClientReadyPacketReceived);

            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var type = (PacketType) dataReader.GetByte();
                Logger.Trace($"Received packet with type {type}");
                switch (type)
                {
                    case PacketType.SkirmishSlotStatus:
                        _processor.ReadPacket(dataReader, fromPeer.EndPoint);
                        break;

                    case PacketType.SkirmishClientReady:
                        _processor.ReadPacket(dataReader);
                        break;

                    case PacketType.SkirmishStartGame:
                        _processor.ReadPacket(dataReader);
                        break;

                    default:
                        Debug.Assert(false, $"Client should never receive {type} packages");
                        Logger.Error($"Client should never receive {type} packages");
                        break;
                }
            };

            Logger.Trace($"Joining game at {endPoint}");

            _manager.Start(Ports.AnyAvailable);

            _writer.Reset();
            _processor.Write(_writer, new SkirmishClientConnectPacket()
            {
                PlayerName = _game.LobbyManager.Username,
                ClientId = ClientInstance.Id,
            });

            _manager.Connect(endPoint.Address.ToString(), Ports.SkirmishHost, _writer);

            StartThread();
        }

        public override bool IsStartButtonEnabled() => Settings.LocalSlot?.Ready == false;

        public override Task HandleStartButtonClickAsync()
        {
            Settings.LocalSlot.Ready = true;
            Settings.LocalSlot.ReadyUpdated = true;
            return Task.CompletedTask;
        }

        protected override async Task CreateNetworkConnectionAsync()
        {
            var connection = new ClientNetworkConnection(Settings);
            await connection.InitializeAsync();

            Connection = connection;
        }

        protected override void Loop()
        {
            switch (Settings.Status)
            {
                case SkirmishGameStatus.Configuring:
                    var localSlot = Settings.LocalSlot;
                    if (localSlot != null)
                    {
                        if (localSlot.IsDirty)
                        {
                            Logger.Trace($"Local slot is dirty, sending...");
                            _writer.Put((byte) PacketType.SkirmishClientUpdate);
                            _processor.Write(_writer, new SkirmishClientUpdatePacket()
                            {
                                PlayerName = localSlot.PlayerName,
                                ColorIndex = localSlot.ColorIndex,
                                FactionIndex = localSlot.FactionIndex,
                                Team = localSlot.Team,
                                StartPosition = localSlot.StartPosition
                            });

                            _manager.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
                            localSlot.ResetDirty();
                        }

                        if (localSlot.ReadyUpdated)
                        {
                            Logger.Trace($"Sending ready...");
                            _writer.Reset();
                            _writer.Put((byte) PacketType.SkirmishClientReady);
                            _manager.SendToAll(_writer, DeliveryMethod.ReliableOrdered);

                            localSlot.ReadyUpdated = false;
                        }
                    }


                    break;

                case SkirmishGameStatus.ReadyToStart:
                case SkirmishGameStatus.Started:
                    Stop();
                    break;
            }
        }
    }

    public sealed class HostSkirmishManager : NetworkSkirmishManager
    {
        private Dictionary<int, SkirmishSlot> _slotLookup = new Dictionary<int, SkirmishSlot>();

        private void SkirmishClientUpdatePacketReceived(SkirmishClientUpdatePacket packet, SkirmishSlot slot)
        {
            if (slot != null)
            {
                slot.PlayerName = packet.PlayerName;
                slot.Team = packet.Team;
                slot.FactionIndex = packet.FactionIndex;
                slot.ColorIndex = packet.ColorIndex;

                if (packet.StartPosition == 0 || packet.StartPosition == slot.StartPosition || !Settings.Slots.Any(s => s.StartPosition == packet.StartPosition))
                {
                    slot.StartPosition = packet.StartPosition;
                }
            }
        }

        private void SkirmishClientConnectPacketReceived(SkirmishClientConnectPacket packet, SkirmishSlot slot)
        {
            slot.ClientId = packet.ClientId;
            slot.PlayerName = packet.PlayerName;
        }

        public HostSkirmishManager(Game game) : base(game, isHosting: true)
        {
            _processor.SubscribeReusable<SkirmishClientConnectPacket, SkirmishSlot>(SkirmishClientConnectPacketReceived);
            _processor.SubscribeReusable<SkirmishClientUpdatePacket, SkirmishSlot>(SkirmishClientUpdatePacketReceived);

            _listener.PeerConnectedEvent += peer => Logger.Trace($"{peer.EndPoint} connected");

            _listener.PeerDisconnectedEvent += (peer, info) =>
            {
                Logger.Trace($"{peer.EndPoint} disconnected with reason {info.Reason}");

                var slot = _slotLookup[peer.Id];
                slot.State = SkirmishSlotState.Open;
                slot.ClientId = null;
                slot.PlayerName = null;
                slot.Ready = false;

                _slotLookup.Remove(peer.Id);
            };

            _listener.ConnectionRequestEvent += request =>
            {
                var nextFreeSlot = Settings.Slots.FirstOrDefault(s => s.State == SkirmishSlotState.Open);
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
                    case PacketType.SkirmishClientUpdate:
                        _processor.ReadPacket(dataReader, slot);
                        break;
                    case PacketType.SkirmishClientReady:
                        slot.Ready = true;
                        slot.ReadyUpdated = true;
                        break;
                    default:
                        Debug.Assert(false, $"Host should never receive {type} packages");
                        Logger.Error($"Host should never receive {type} packages");
                        break;
                }
            };

            Settings.LocalSlotIndex = 0;

            var localSlot = Settings.LocalSlot;
            localSlot.PlayerName = _game.LobbyManager.Username;
            localSlot.State = SkirmishSlotState.Human;
            localSlot.EndPoint = new IPEndPoint(IPAddress.Any, Ports.SkirmishHost); // The host does not know his own external IP address

            _manager.Start(Ports.SkirmishHost);

            StartThread();
        }

        public override bool IsStartButtonEnabled()                  
        {
            //all human players (except for the host) are ready
            return Settings.Slots.Where(s => s.State == SkirmishSlotState.Human && s.Index != 0)
                                 .All(s => s.Ready);
        }

        public override async Task HandleStartButtonClickAsync()
        {
            Settings.Status = SkirmishGameStatus.SendingStartSignal;
            await CreateNetworkConnectionAsync();
        }

        protected override async Task CreateNetworkConnectionAsync()
        {
            var connection = new HostNetworkConnection(Settings);
            await connection.InitializeAsync();

            Connection = connection;
        }

        protected override void Loop()
        {
            switch (Settings.Status)
            {
                case SkirmishGameStatus.Configuring:
                    if (Settings.IsDirty)
                    {
                        _writer.Put((byte) PacketType.SkirmishSlotStatus);
                        _processor.Write(_writer, new SkirmishGameStatusPacket()
                        {
                            MapName = Settings.MapName,
                            Slots = Settings.Slots
                        });

                        _manager.SendToAll(_writer, DeliveryMethod.ReliableOrdered);

                        Settings.ResetDirty();

                        foreach (var slot in Settings.Slots)
                        {
                            slot.Ready = false;
                        }
                    }

                    foreach (var slot in Settings.Slots)
                    {
                        if (slot.ReadyUpdated)
                        {
                            _writer.Put((byte) PacketType.SkirmishClientReady);
                            _processor.Write(_writer, new SkirmishClientReadyPacket()
                            {
                                Index = slot.Index
                            });

                            _manager.SendToAll(_writer, DeliveryMethod.ReliableOrdered);

                            slot.ReadyUpdated = false;
                        }
                    }

                    break;

                case SkirmishGameStatus.SendingStartSignal:
                    _writer.Put((byte) PacketType.SkirmishStartGame);
                    _processor.Write(_writer, new SkirmishStartGamePacket()
                    {
                        Seed = Settings.Seed
                    });

                    _manager.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
                    Settings.Status = SkirmishGameStatus.WaitingForAllPlayers;
                    break;

                case SkirmishGameStatus.WaitingForAllPlayers:
                    if (Connection != null)
                    {
                        Settings.Status = SkirmishGameStatus.ReadyToStart;
                    }
                    break;

                case SkirmishGameStatus.ReadyToStart:
                case SkirmishGameStatus.Started:
                    Stop();
                    break;
            }
        }

        public override void Stop()
        {
            this.IsHosting = false;
            base.Stop();
        }
    }
}
