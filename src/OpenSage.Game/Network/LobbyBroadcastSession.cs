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
        private LobbyManager _lobbyManager;
        private bool _running;

        public LobbyBroadcastSession(LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
            
            _sock = AddDisposable(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp));
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);

            _broadcastAddr = new IPEndPoint(LobbyManager.GetBroadcastAddress(lobbyManager.Unicast), Ports.LobbyScan);
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
            if (_lobbyManager.Hosting)
            {
                msg = new LobbyProtocol.LobbyGameMessage()
                {
                    InLobby = true,
                    Map = _lobbyManager.Map,
                    Name = _lobbyManager.Username,
                    Players = 1
                };
            }
            else
            {
                msg = new LobbyProtocol.LobbyMessage()
                {
                    InLobby = _lobbyManager.InLobby,
                    Name = _lobbyManager.Username,
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
