using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSage.Network
{
    public class LobbyScanSession
    {
        private UdpClient _client;
        private IPEndPoint _receiveEp;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class LobbyScannedEventArgs : EventArgs
        {
            public string Host { get; set; }
            public string Name { get; set; }
            public string Map { get; set; }
        }

        public delegate void LobbyScannedEventHandler(Object sender, LobbyScannedEventArgs e);
        public event LobbyScannedEventHandler LobbyDetected;

        public LobbyScanSession()
        {
            _receiveEp = new IPEndPoint(IPAddress.Any, Ports.LobbyScan);
            _client = new UdpClient();
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.ExclusiveAddressUse = false;
        }

        public async void Start()
        {
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
                        //Task is done / was canceld
                        break;
                    }
                }
            });
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
            _client.Close();

        }

        private void ProcessReceive(UdpReceiveResult result)
        {
            byte[] receiveBytes = result.Buffer;
            string receiveString = Encoding.ASCII.GetString(receiveBytes);

            logger.Info($"Received broadcast: {receiveString}");
            LobbyScannedEventArgs args = new LobbyScannedEventArgs();
            args.Host = receiveString;
            args.Name = receiveString;
            LobbyDetected?.Invoke(this, args);
        }
    }
}
