using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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

        public bool Updated { get; set; }
        public bool InLobby { get; set; }
        public bool Hosting { get; set; }

        public LobbyManager()
        {
            Games = new Dictionary<IPEndPoint, LobbyGame>();
            Players = new Dictionary<IPEndPoint, LobbyPlayer>();
            Username = Environment.MachineName;
            InLobby = false;
            Hosting = false;
            Updated = true;
            var selfAdresses = Dns.GetHostAddresses(Dns.GetHostName());
            LocalIPAdress = selfAdresses.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}
