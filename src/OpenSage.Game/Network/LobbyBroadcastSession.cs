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
                while (_running)
                {
                    Broadcast();
                    try
                    {
                        await Task.Delay(1000, _cancelToken);
                    }
                    catch (TaskCanceledException e)
                    {
                        //maybe refresh the token
                        if (_running)
                        {
                            _cancelTokenSource = new CancellationTokenSource();
                            _cancelToken = _cancelTokenSource.Token;
                        }
                    }
                }
            });
        }

        public void Bump()
        {
            if(_cancelTokenSource != null)
            {
                // interrupt the sleeping of the thread, to enable quick rebroadcast
                _cancelTokenSource.Cancel();
            }
        }

        public void Stop()
        {
            _running = false;
            _cancelTokenSource.Cancel();
        }

        private void Broadcast()
        {
            LobbyProtocol.LobbyMessage msg = null;

            msg = new LobbyProtocol.LobbyMessage()
            {
                IsHosting = _lobbyManager.Hosting,
                Name = _lobbyManager.Username,
            };

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
