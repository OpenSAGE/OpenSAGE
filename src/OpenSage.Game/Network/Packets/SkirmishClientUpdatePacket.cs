namespace OpenSage.Network.Packets
{
    public class SkirmishClientUpdatePacket
    {
        public string PlayerName { get; set; }
        public byte ColorIndex { get; set; }
        public byte FactionIndex { get; set; }
        public byte Team { get; set; }
        public byte StartPosition { get; set; }
        public bool Ready { get; set; }
    }
}
