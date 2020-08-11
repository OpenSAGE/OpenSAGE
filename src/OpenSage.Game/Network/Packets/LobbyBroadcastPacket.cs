namespace OpenSage.Network.Packets
{
    public class LobbyBroadcastPacket
    {
        // Temporary for debugging
        public int ProcessId { get; set; }
        public string Username { get; set; }
        public bool IsHosting { get; set; }
    }
}
