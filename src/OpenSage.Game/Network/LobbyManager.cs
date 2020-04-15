using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace OpenSage.Network
{
    public class LobbyManager
    {
        public struct LobbyPlayer
        {
            public string Name { get; set; }
            public bool IsHosting { get; set; }
            public IPEndPoint Endpoint { get; set; }
            public DateTime LastSeen;
        }

        public Dictionary<IPEndPoint, LobbyPlayer> Players { get; }

        private string _username;
        public string Username {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                LobbyBroadcastSession.Bump();
            }
        }

        public string Map { get; set; }

        public UnicastIPAddressInformation Unicast { get; set; }

        public bool Updated { get; set; }
        public bool Hosting { get; set; }

        public LobbyBroadcastSession LobbyBroadcastSession { get; }
        public LobbyScanSession LobbyScanSession { get; }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public LobbyManager(Game game)
        {

            var localIp = game.Configuration.LanIpAddress;

            if(localIp == IPAddress.Any){
                var selfAdresses = Dns.GetHostAddresses(Dns.GetHostName());
                localIp = selfAdresses.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }

            Unicast = GetLocalAdapter(localIp);

            LobbyBroadcastSession = new LobbyBroadcastSession(this);
            LobbyScanSession = new LobbyScanSession(this);

            Players = new Dictionary<IPEndPoint, LobbyPlayer>();
            Username = Environment.MachineName;
            Hosting = false;
            Updated = true;

        }

        public void Start()
        {
            Hosting = false;
            LobbyBroadcastSession.Start();
            LobbyScanSession.Start();
        }

        public void Stop()
        {
            LobbyBroadcastSession.Stop();
            LobbyScanSession.Stop();
        }

        public static IPAddress GetBroadcastAddress(UnicastIPAddressInformation unicastAddress)
        {
            return GetBroadcastAddress(unicastAddress.Address, unicastAddress.IPv4Mask);
        }

        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress mask)
        {
            uint ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
        }

        internal static UnicastIPAddressInformation GetLocalAdapter(IPAddress ipAddress)
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) // Iterate over each network interface
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {   // Fetch the properties of this adapter

                    IPInterfaceProperties adapterProperties = item.GetIPProperties();
                    var relevantUnicast = adapterProperties.UnicastAddresses.Where(x => x.Address.Equals(ipAddress)).FirstOrDefault();
                    
                    if(relevantUnicast != null){
                        return relevantUnicast;
                    }

                }
            }
            return null;
        }

    }
}
