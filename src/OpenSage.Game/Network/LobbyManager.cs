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
        }

        public struct LobbyGame
        {
            public string Name { get; set; }
        }

        public Dictionary<IPEndPoint, LobbyGame> Games { get; }
        public Dictionary<IPEndPoint, LobbyPlayer> Players { get; }

        public string Username { get; set; }
        public string Map { get; set; }
        public IPAddress LocalIPAdress { get; set; }
        public UnicastIPAddressInformation Unicast { get; set; }

        public bool Updated { get; set; }
        public bool InLobby { get; set; }
        public bool Hosting { get; set; }

        public LobbyBroadcastSession LobbyBroadcastSession { get; }
        public LobbyScanSession LobbyScanSession { get; }

        public LobbyManager(Game game)
        {

            LocalIPAdress = game.Configuration.LanIpAddress;

            if(LocalIPAdress == IPAddress.Any){
                var selfAdresses = Dns.GetHostAddresses(Dns.GetHostName());
                LocalIPAdress = selfAdresses.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }

            Unicast = GetLocalAdapter(LocalIPAdress);

            LobbyBroadcastSession = new LobbyBroadcastSession(this);
            LobbyScanSession = new LobbyScanSession(this);

            Games = new Dictionary<IPEndPoint, LobbyGame>();
            Players = new Dictionary<IPEndPoint, LobbyPlayer>();
            Username = Environment.MachineName;
            InLobby = false;
            Hosting = false;
            Updated = true;


            
        }

        public void Start()
        {
            LobbyBroadcastSession.Start();
            LobbyScanSession.Start();
        }

        public void Stop()
        {
            LobbyBroadcastSession.Stop();
            LobbyScanSession.Stop();
        }


        public delegate void LobbyGameScannedEventHandler(object sender, LobbyScanSession.LobbyGameScannedEventArgs e);
        public event LobbyGameScannedEventHandler LobbyGameDetected;
        public void FireLobbyGameDetected(LobbyScanSession.LobbyGameScannedEventArgs args)
        {
            this.LobbyGameDetected?.Invoke(this, args);
        }

        public delegate void LobbyPlayerScannedEventHandler(object sender, LobbyScanSession.LobbyPlayerScannedEventArgs e);
        public event LobbyPlayerScannedEventHandler LobbyPlayerDetected;
        public void FireLobbyPlayerDetected(LobbyScanSession.LobbyPlayerScannedEventArgs args)
        {
            this.LobbyPlayerDetected?.Invoke(this, args);
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
