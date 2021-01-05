using System;
using System.Net;

namespace OpenSage.Network
{
    public class LobbyPlayer
    {
        public string ClientId { get; internal set; }
        public string Username { get; set; }
        public bool IsHosting { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
