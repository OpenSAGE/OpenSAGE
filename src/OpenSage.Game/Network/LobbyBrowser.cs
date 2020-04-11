using System;
using System.Collections.Generic;
using System.Net;

namespace OpenSage.Network
{
    public class LobbyBrowser
    {
        public struct LobbyPlayer
        {
            public string Name { get; }
        }

        public struct LobbyGame
        {
            public string Name { get; set; }
        }

        public Dictionary<IPEndPoint, LobbyGame> Games { get; }
        public List<LobbyPlayer> Players { get; }

        public string Username { get; set; }

        public bool Updated { get; set; }

        public LobbyBrowser()
        {
            Games = new Dictionary<IPEndPoint, LobbyGame>();
            Players = new List<LobbyPlayer>();
            Username = Environment.MachineName;
            Updated = true;
        }
    }
}
