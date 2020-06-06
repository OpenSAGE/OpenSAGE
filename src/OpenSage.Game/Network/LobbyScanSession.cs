using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace OpenSage.Network
{
    public class LobbyScanSession
    {
        private UdpClient _client;
        private IPEndPoint _receiveEp;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private LobbyManager _lobbyManager;
        private bool _running;

        public class LobbyGameScannedEventArgs : EventArgs
        {
            public IPEndPoint Host { get; set; }
            public string Name { get; set; }
            public string Map { get; set; }
        }

        public class LobbyPlayerScannedEventArgs : EventArgs
        {
            public IPEndPoint Host { get; set; }
            public string Name { get; set; }
            public string Map { get; set; }
        }


        public LobbyScanSession(LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
            _receiveEp = new IPEndPoint(lobbyManager.Unicast.Address, Ports.LobbyScan);
            _running = false;
        }

        public async void Start()
        {
            if (_running)
            {
                return;
            }
            _running = true;

            _client = new UdpClient();
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.Bind(_receiveEp);

            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            await Task.Run(async () =>
            {
                while (_running)
                {
                    try
                    {
                        CleanExpiredPlayers();
                        var result = await _client.ReceiveAsync().WithCancellation(_cancelToken);
                        ProcessReceive(result);
                    }
                    catch (OperationCanceledException)
                    {
                        //Task is done / was cancelled
                        logger.Info("Cancelled");
                    }
                }
            });
        }

        private void CleanExpiredPlayers()
        {
            var threshold = DateTime.Now.Subtract(new TimeSpan(0, 0, 3));

            var expireds = _lobbyManager.Players.Where(x => x.Value.LastSeen < threshold);

            foreach (var expired in expireds)
            {
                _lobbyManager.Updated = true;
                _lobbyManager.Players.Remove(expired.Key);
            }
        }

        public void Stop()
        {
            _running = false;
            _cancelTokenSource.Cancel();
            _client.Client.Close();
        }

        private void ProcessReceive(UdpReceiveResult result)
        {
            byte[] receiveBytes = result.Buffer;
            string ascii = Encoding.UTF32.GetString(receiveBytes);
            using (var receiveStream = new MemoryStream(receiveBytes))
            {
                // Deserialize response
                var response = Serializer.Deserialize<LobbyProtocol.LobbyMessage>(receiveStream);

                logger.Info($"Received broadcast from: {result.RemoteEndPoint.Address}({response.Name})");

                switch (response)
                {
                    case LobbyProtocol.LobbyMessage lobbyMessage:

                        var endpoint = result.RemoteEndPoint;

                        if (!_lobbyManager.Players.TryGetValue(endpoint, out var player))
                        {
                            player = _lobbyManager.Players[endpoint] = new LobbyManager.LobbyPlayer();
                        }

                        player.Endpoint = endpoint;
                        player.Name = lobbyMessage.Name;
                        player.IsHosting = lobbyMessage.IsHosting;
                        var updated = !player.Equals(_lobbyManager.Players[endpoint]);

                        player.LastSeen = DateTime.Now;

                        _lobbyManager.Players[endpoint] = player;

                        _lobbyManager.Updated |= updated;

                        break;

                    default:
                        logger.Error($"Unknown message: {response.GetType().Name}");
                        break;
                }

            }
        }
    }
}
