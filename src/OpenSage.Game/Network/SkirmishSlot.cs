using System.Net;
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
        public IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// We need this during development to be able to run two games on the same machine.
        /// </summary>
        public int ProcessId { get; set; }

        public static SkirmishSlot Deserialize(NetDataReader reader)
        {
            var slot = new SkirmishSlot()
            {
                Index = reader.GetInt(),
                State = (SkirmishSlotState) reader.GetByte(),
                PlayerName = reader.GetString(),
                ColorIndex = reader.GetByte(),
                FactionIndex = reader.GetByte(),
                Team = reader.GetByte(),
                Ready = reader.GetBool()
            };

            if (slot.State == SkirmishSlotState.Human)
            {
                slot.EndPoint = reader.GetNetEndPoint();
                slot.ProcessId = reader.GetInt();
            }

            return slot;
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
            if (slot.State == SkirmishSlotState.Human)
            {
                writer.Put(slot.EndPoint);
                writer.Put(slot.ProcessId);
            }
        }
    }
}
