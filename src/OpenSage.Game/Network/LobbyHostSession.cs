using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace OpenSage.Network
{
    public class LobbyHostSession : DisposableBase
    {
        private Socket _sock;
        private IPEndPoint _broadcastAddr;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Game _game;


        public LobbyHostSession(Game game)
        {
            _sock = AddDisposable(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp));
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _broadcastAddr = new IPEndPoint(IPAddress.Broadcast, Ports.LobbyScan);
            _game = game;
        }

        public async void Start()
        {
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = _cancelTokenSource.Token;

            await Task.Run(async () =>
            {
                while (true)
                {
                    Broadcast();
                    await Task.Delay(1000, _cancelToken);
                    if (_cancelToken.IsCancellationRequested)
                        break;
                }
            });
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
        }

        private void Broadcast()
        {
            var broadcast = new LobbyProtocol.LobbyBroadcast();
            broadcast.Name = _game.LobbyBrowser.Username;

            var formatter = new BinaryFormatter();
            using (var output = new MemoryStream())
            //using (var compress = new BrotliStream(output, CompressionMode.Compress))
            {
                Serializer.Serialize(output, broadcast);
                byte[] data = output.ToArray();
                _sock.SendTo(data, _broadcastAddr);
                logger.Info("Sending broadcast!");
            }
        }

    }
}
