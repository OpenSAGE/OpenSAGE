using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ProtoBuf;

namespace OpenSage.Network
{
    public class SkirmishManager
    {
        public class SkirmishClient
        {
            TcpClient Client;
            string Name = "";
            private SkirmishManager _manager;
            private TcpClient _client;
            private CancellationTokenSource _cancelTokenSource;
            private CancellationToken _cancelToken;

            public SkirmishSlot Slot { get
                {
                    return _manager._slots.Where(x => x.Client == this).First();
                }
            }

            public SkirmishClient(SkirmishManager manager, TcpClient client)
            {
                _manager = manager;
                _client = client;

                var result = _manager.AddClient(this);

                if (!result)
                {
                    client.Close();
                    client.Dispose();
                }

            }

            public async void Run()
            {

                var stream = _client.GetStream();
                while (_client.Connected)
                {
                    if (_cancelTokenSource == null || _cancelTokenSource.IsCancellationRequested)
                    {
                        _cancelTokenSource = new CancellationTokenSource();
                        _cancelToken = _cancelTokenSource.Token;
                    }
                    var msg = await Task.Run(() => Serializer.Deserialize<SkirmishProtocol.UpdateState>(stream), _cancelToken);

                    Slot.ColorIndex = msg.ColorIndex;
                    Slot.FactionIndex = msg.FactionIndex;
                    Slot.HumanName = msg.Name;

                    Slot.Updated = true;
                }

            }
        }

        private Game _game;
        private List<SkirmishSlot> _slots;
        private TcpListener _server;
        private bool _running;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;
        public const int Port = 8087;

        public string Map;

        public class SkirmishSlot
        {
            public enum SlotType
            {
                Closed,
                Open,
                Human,
                EasyArmy,
                MediumArmy,
                HardArmy,
            }

            public enum TeamType
            {
                None = 0,
                Team1 = 1,
                Team2 = 2,
                Team3 = 3,
                Team4 = 4
            }

            public SlotType Type;
            public int ColorIndex;
            public int FactionIndex;
            public TeamType Team;
            public bool Disabled
            {
                get {
                    return Disabled;
                }
                set
                {
                    Type = SlotType.Closed;
                    Disabled = value;
                }
            }

            public string HumanName;

            public SkirmishClient Client { get; internal set; }
            public bool Updated;

        }

        public SkirmishManager(Game game)
        {
            _game = game;

            _slots = new List<SkirmishSlot>(8);
        }

        public async void Start()
        {

            _running = true;

            var unicast = _game.LobbyManager.Unicast;
            _server = new TcpListener(unicast.Address, Port);
            _server.Start();

            while (_running)
            {
                if (_cancelTokenSource == null || _cancelTokenSource.IsCancellationRequested)
                {
                    _cancelTokenSource = new CancellationTokenSource();
                    _cancelToken = _cancelTokenSource.Token;
                }

                var client = await Task.Run(
                    () => _server.AcceptTcpClientAsync(),
                    _cancelToken);

                new SkirmishClient(this, client);


            }
            _server.Stop();
        }

        private bool AddClient(SkirmishClient skirmishClient)
        {
            var slot = _slots.Where(x => x.Type == SkirmishSlot.SlotType.Open).First();
            if(slot == null)
            {
                return false;
            }

            slot.Type = SkirmishSlot.SlotType.Human;
            slot.Client = skirmishClient;

            skirmishClient.Run();


            return true;

        }

        public void Stop()
        {
            _running = false;
            _cancelTokenSource.Cancel();
        }
    }
}
