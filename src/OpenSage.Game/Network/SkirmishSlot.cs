using LiteNetLib.Utils;

namespace OpenSage.Network
{
    public enum SkirmishSlotState : byte
    {
        Closed,
        Open,
        Human,
        EasyArmy,
        MediumArmy,
        HardArmy,
    }

    public class SkirmishSlot
    {
        public SkirmishSlot()
        {
        }

        public SkirmishSlot(int index)
        {
            Index = index;
        }

        public int Index { get; set; }
        public SkirmishSlotState State { get; set; } = SkirmishSlotState.Open;
        public string PlayerName { get; set; }
        public byte ColorIndex { get; set; }
        public byte FactionIndex { get; set; }
        public byte Team { get; set; }
        public bool Ready { get; set; }
        public int PeerId { get; set; }

        public static SkirmishSlot Deserialize(NetDataReader reader)
        {
            return new SkirmishSlot()
            {
                Index = reader.GetInt(),
                State = (SkirmishSlotState) reader.GetByte(),
                PlayerName = reader.GetString(),
                ColorIndex = reader.GetByte(),
                FactionIndex = reader.GetByte(),
                Team = reader.GetByte(),
                Ready = reader.GetBool(),
                PeerId = reader.GetInt()
            };
        }

        public static void Serialize(NetDataWriter writer, SkirmishSlot slot)
        {
            writer.Put(slot.Index);
            writer.Put((byte) slot.State);
            writer.Put(slot.PlayerName);
            writer.Put(slot.ColorIndex);
            writer.Put(slot.FactionIndex);
            writer.Put(slot.Team);
            writer.Put(slot.Ready);
            writer.Put(slot.PeerId);
        }
    }
}
