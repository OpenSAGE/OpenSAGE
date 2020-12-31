namespace OpenSage.Network.Packets
{
    public class SkirmishClientConnectPacket
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int ProcessId { get; set; }
    }
}
