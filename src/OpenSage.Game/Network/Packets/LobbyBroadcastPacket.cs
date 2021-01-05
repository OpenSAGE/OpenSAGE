namespace OpenSage.Network.Packets
{
    public class LobbyBroadcastPacket
    {
        public string ClientId { get; set; }
        public string Username { get; set; }
        public bool IsHosting { get; set; }
    }
}
