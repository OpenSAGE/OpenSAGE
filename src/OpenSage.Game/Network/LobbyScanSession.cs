using System;
using System.IO;
using System.Net;
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
            
            _receiveEp = new IPEndPoint(lobbyManager.LocalIPAdress, Ports.LobbyScan);
            _client = new UdpClient();
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            // _client.ExclusiveAddressUse = true;
            _running = false;
        }

        public async void Start()
        {
            if (_running)
            {
                return;
            }
            _running = true;
            _client.Client.Bind(_receiveEp);
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var result = await _client.ReceiveAsync().WithCancellation(_cancelToken);
                        ProcessReceive(result);
                    }
                    catch (OperationCanceledException)
                    {
                        //Task is done / was cancelled
                        break;
                    }
                }
            });
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
            _client.Client.Close();
            _running = false;
        }

        private void ProcessReceive(UdpReceiveResult result)
        {
            byte[] receiveBytes = result.Buffer;
            string ascii = Encoding.UTF32.GetString(receiveBytes);
            using (var receiveStream = new MemoryStream(receiveBytes))
            {
                // Deserialize response
                var response = Serializer.Deserialize<LobbyProtocol.LobbyMessage>(receiveStream);

                // Check if is localhost
                if (IPAddress.IsLoopback(result.RemoteEndPoint.Address) ||
                    result.RemoteEndPoint.Address.Equals(_lobbyManager.LocalIPAdress))
                {
                    logger.Debug($"Skipping: Received broadcast from localhost");
                    //return;
                }
                else
                {
                    logger.Info($"Received broadcast from: {result.RemoteEndPoint.Address}({response.Name})");
                }

                if (response is LobbyProtocol.LobbyGameMessage)
                {
                    // Add the game to our lobby
                    var lobbyGame = new LobbyManager.LobbyGame();
                    lobbyGame.Name = response.Name;

                    if (!_lobbyManager.Games.ContainsKey(result.RemoteEndPoint))
                    {
                        _lobbyManager.Games.Add(result.RemoteEndPoint, lobbyGame);
                    }

                    // Fire event
                    var args = new LobbyGameScannedEventArgs();
                    args.Host = result.RemoteEndPoint;
                    args.Name = response.Name;
                    _lobbyManager.FireLobbyGameDetected(args);
                }
                else if (!response.InLobby)
                {
                    // Add the game to our lobby
                    var lobbyPlayer = new LobbyManager.LobbyPlayer();
                    lobbyPlayer.Name = response.Name;

                    if (!_lobbyManager.Players.ContainsKey(result.RemoteEndPoint))
                    {
                        _lobbyManager.Players.Add(result.RemoteEndPoint, lobbyPlayer);
                    }

                    // Fire event
                    var args = new LobbyPlayerScannedEventArgs();
                    args.Host = result.RemoteEndPoint;
                    _lobbyManager.FireLobbyPlayerDetected(args);
                }
            }
        }
    }
}
