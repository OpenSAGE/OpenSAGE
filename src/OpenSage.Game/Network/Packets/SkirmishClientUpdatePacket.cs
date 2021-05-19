namespace OpenSage.Network.Packets
{
    public class SkirmishClientUpdatePacket
    {
        public string PlayerName { get; set; }
        public sbyte ColorIndex { get; set; }
        public byte FactionIndex { get; set; }
        public sbyte Team { get; set; }
        public byte StartPosition { get; set; }
    }
}
