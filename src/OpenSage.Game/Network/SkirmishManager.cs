using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ProtoBuf;
using System.IO;

namespace OpenSage.Network
{
    public class SkirmishManager
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class SkirmishClient
        {

            private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            }

            public void Close()
            {

                _client.Close();
                _client.Dispose();
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

                    logger.Info("Have new UpdateState message");

                    var slot = Slot;
                    slot.Type = msg.Slot.Type;
                    slot.Team = msg.Slot.Team;
                    slot.ColorIndex = msg.Slot.ColorIndex;
                    slot.FactionIndex = msg.Slot.FactionIndex;
                    slot.HumanName = msg.Slot.Name;
                    slot.Ready = msg.Slot.Ready;

                    Slot.Updated = true;

                    _manager.Sync();
                }

            }

            internal void Send(byte[] statePacket)
            {
                if(_client != null)
                {
                    _client.GetStream().Write(statePacket);
                }
            }
        }


        private Game _game;
        private List<SkirmishSlot> _slots = new List<SkirmishSlot>(8);
        private TcpListener _server;
        private bool _running;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;
        public const int Port = 8087;

        public string Map;

        public bool Hosting = false;

        private TcpClient _serverConnection;
        private SkirmishSlot ownSlot;

        public List<SkirmishSlot> Slots => _slots;

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
            public byte ColorIndex;
            public byte FactionIndex;
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

            public bool Ready {
                get {
                    return true;
                }
                set
                {

                }
            }

            public SkirmishClient Client { get; internal set; }
            public bool Updated;

            public int id;

        }

        public async void Start()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            var _cancelTokenSource = new CancellationTokenSource();
            var _cancelToken = _cancelTokenSource.Token;

            await Task.Run(async () =>
            {
                while (_running)
                {
                    Sync();
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

        public async void Join(IPEndPoint endpoint)
        {
            _serverConnection = new TcpClient();
            _serverConnection.Connect(endpoint.Address, Port);

            Start();

            while (_running)
            {
                if (_cancelTokenSource == null || _cancelTokenSource.IsCancellationRequested)
                {
                    _cancelTokenSource = new CancellationTokenSource();
                    _cancelToken = _cancelTokenSource.Token;
                }

                var msg = await Task.Run(() => Serializer.Deserialize<SkirmishProtocol.SkirmishState>(_serverConnection.GetStream()), _cancelToken);

                logger.Info("Have new Skirmish State");

            }
            _serverConnection.Close();
        }

        public void Sync()
        {
            if (Hosting)
            {
                //TODO: send full state packet to all clients
                var statePacket = new SkirmishProtocol.SkirmishState();
                statePacket.Map = "Map!";

                statePacket.Slots = _slots.Select(x =>
                {
                    var skirmishSlot = new SkirmishProtocol.SkirmishSlot
                    {
                        ColorIndex = x.ColorIndex,
                        FactionIndex = x.FactionIndex,
                        Name = x.HumanName,
                        Ready = x.Ready,
                        Type = x.Type,
                        Team = x.Team
                    };
                    return skirmishSlot;
                }).ToArray();


                using (var output = new MemoryStream())
                //using (var compress = new BrotliStream(output, CompressionMode.Compress))
                {
                    Serializer.Serialize(output, statePacket);
                    byte[] data = output.ToArray();

                    _slots.ForEach(x =>
                    {
                        if (x.Client != null)
                        {
                            x.Client.Send(data);
                        }
                    });
                }
            }
            else
            {
                var selfPacket = new SkirmishProtocol.UpdateState();

                var skirmishSlot = new SkirmishProtocol.SkirmishSlot();
                skirmishSlot.ColorIndex = ownSlot.ColorIndex;
                skirmishSlot.FactionIndex = ownSlot.FactionIndex;
                skirmishSlot.Name = ownSlot.HumanName;
                skirmishSlot.Ready = ownSlot.Ready;
                skirmishSlot.Type = ownSlot.Type;
                skirmishSlot.Team = ownSlot.Team;
                selfPacket.Slot = skirmishSlot;


                Serializer.Serialize(_serverConnection.GetStream(), selfPacket);
            }
        }

        public SkirmishManager(Game game)
        {
            _game = game;

            for (var a = 0; a < 8; a++)
            {
                var slot = new SkirmishSlot();
                slot.Type = SkirmishSlot.SlotType.Closed;
                slot.id = a;
                _slots.Add(slot);
            }
            
        }

        

        public async void Host()
        {
            Start();

            Hosting = true;

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

                var skirmishClient = new SkirmishClient(this, client);

                var result = this.AddClient(skirmishClient);
                if (!result)
                {
                    skirmishClient.Close();
                }


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
            _cancelTokenSource?.Cancel();
        }
    }
}
