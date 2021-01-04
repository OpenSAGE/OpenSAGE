namespace OpenSage.Network.Packets
{
    public class SkirmishGameStatusPacket
    {
        public string MapName { get; set; }
        public SkirmishSlot[] Slots { get; set; }
    }
}
