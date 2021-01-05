using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using OpenSage.Network.Packets;

namespace OpenSage.Network
{
    public class LobbyManager
    {
        private Game _game;
        private EventBasedNetListener _listener;
        private NetManager _manager;
        private Thread _thread;
        private bool _isRunning;
        private NetPacketProcessor _processor;
        private List<LobbyPlayer> _players = new List<LobbyPlayer>();

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public IReadOnlyCollection<LobbyPlayer> Players => _players;

        public LobbyManager(Game game)
        {
            _game = game;

            _listener = new EventBasedNetListener();
            _manager = new NetManager(_listener)
            {
                BroadcastReceiveEnabled = true,
                ReuseAddress = true
            };

            _processor = new NetPacketProcessor();
            _processor.SubscribeReusable<LobbyBroadcastPacket, IPEndPoint>(LobbyBroadcastReceived);

            _listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnectedEvent;
        }

        public string Username { get; set; } = Environment.MachineName;

        public void Start()
        {
            _manager.Start(Ports.LobbyScan);

            _isRunning = true;
            _thread = new Thread(Loop)
            {
                IsBackground = true,
                Name = "OpenSAGE Lobby Manager"
            };
            _thread.Start();
        }

        public void Stop()
        {
            _manager.Stop();

            _isRunning = false;
            _thread = null;
        }

        private void Loop()
        {
            var writer = new NetDataWriter();

            var broadcastPacket = new LobbyBroadcastPacket()
            {
                ClientId = ClientInstance.Id
            };

            while (_isRunning)
            {
                broadcastPacket.Username = Username;
                broadcastPacket.IsHosting = _game.SkirmishManager?.IsHosting ?? false;

                writer.Reset();
                _processor.Write(writer, broadcastPacket);
                _manager.SendBroadcast(writer, Ports.LobbyScan);

                _manager.PollEvents();

                var removedCount = _players.RemoveAll(IsTimedOut);
                if (removedCount > 0)
                {
                    Logger.Info($"Timeout: Removed {removedCount} players from lobby.");
                }

                Thread.Sleep(100);
            }

            bool IsTimedOut(LobbyPlayer player) =>
                (DateTime.Now - player.LastSeen).TotalSeconds >= 3.0;
        }

        private void NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.Broadcast && remoteEndPoint.Port == Ports.LobbyScan)
            {
                _processor.ReadAllPackets(reader, remoteEndPoint);
            }
        }

        private void LobbyBroadcastReceived(LobbyBroadcastPacket packet, IPEndPoint endPoint)
        {
            var player = _players.FirstOrDefault(p => p.ClientId == packet.ClientId);

            if (player == null)
            {
                player = new LobbyPlayer()
                {
                    EndPoint = endPoint,
                    ClientId = packet.ClientId
                };

                _players.Add(player);
            }

            player.Username = packet.Username;
            player.IsHosting = packet.IsHosting;
            player.LastSeen = DateTime.Now;
        }
    }
}
