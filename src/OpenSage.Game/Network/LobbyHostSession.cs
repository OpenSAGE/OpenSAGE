using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSage.Network
{
    public class LobbyHostSession : DisposableBase
    {
        private Socket _sock;
        private IPEndPoint _broadcastAddr;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public LobbyHostSession()
        {
            _sock = AddDisposable(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp));
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _broadcastAddr = new IPEndPoint(IPAddress.Broadcast, Ports.LobbyScan);

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
            byte[] data = Encoding.ASCII.GetBytes("langame");
            _sock.SendTo(data, _broadcastAddr);
            logger.Info("Sending broadcast!");
        }

    }
}
