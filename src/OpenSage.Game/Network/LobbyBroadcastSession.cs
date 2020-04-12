using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace OpenSage.Network
{
    public class LobbyBroadcastSession : DisposableBase
    {
        private Socket _sock;
        private IPEndPoint _broadcastAddr;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Game _game;
        private bool _running;

        public LobbyBroadcastSession(Game game)
        {
            _sock = AddDisposable(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp));
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _broadcastAddr = new IPEndPoint(IPAddress.Broadcast, Ports.LobbyScan);
            _game = game;
            _running = false;
        }

        public async void Start()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            await Task.Run(async () =>
            {
                while (true)
                {
                    Broadcast();
                    try
                    {
                        await Task.Delay(1000, _cancelToken);
                    }
                    catch (TaskCanceledException e)
                    {
                        break;
                    }
                }
            });
        }

        public void Stop()
        {
            _running = true;
            _sock.Close();
            _cancelTokenSource.Cancel();
        }

        private void Broadcast()
        {
            LobbyProtocol.LobbyMessage msg = null;
            if (_game.LobbyManager.Hosting)
            {
                msg = new LobbyProtocol.LobbyGameMessage()
                {
                    InLobby = true,
                    Map = _game.LobbyManager.Map,
                    Name = _game.LobbyManager.Username,
                    Players = 1
                };
            }
            else
            {
                msg = new LobbyProtocol.LobbyMessage()
                {
                    InLobby = _game.LobbyManager.InLobby,
                    Name = _game.LobbyManager.Username,
                };
            }

            var formatter = new BinaryFormatter();
            using (var output = new MemoryStream())
            //using (var compress = new BrotliStream(output, CompressionMode.Compress))
            {
                Serializer.Serialize(output, msg);
                byte[] data = output.ToArray();
                _sock.SendTo(data, _broadcastAddr);
                logger.Info("Sending broadcast!");
            }
        }

    }
}
