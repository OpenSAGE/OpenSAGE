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

        private SkirmishSlotState _state = SkirmishSlotState.Open;
        public SkirmishSlotState State
        {
            get
            {
                return _state;
            }
            set
            {
                IsDirty |= _state != value;
                _state = value;
            }
        }

        private int _index;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                IsDirty |= _index != value;
                _index = value;
            }
        }


        private string _playerName = string.Empty;
        public string PlayerName { get
            {
                return _playerName;
            }
            set
            {
                IsDirty |= _playerName != value;
                _playerName = value;
            }
        }

        private byte _colorIndex;
        public byte ColorIndex
        {
            get
            {
                return _colorIndex;
            }
            set
            {
                IsDirty |= _colorIndex != value;
                _colorIndex = value;
            }
        }

        private byte _factionIndex;
        public byte FactionIndex { get
            {
                return _factionIndex;
            }
            set
            {
                IsDirty |= _factionIndex != value;
                _factionIndex = value;
            }
        }

        private byte _team;
        public byte Team { get {
                return _team; }
            set
            {
                IsDirty |= _team != value;
                _team = value;
            } }

        private bool _ready;
        public bool Ready { get
            {
                return _ready;
            } set
            {
                IsDirty |= _ready != value;
                _ready = value;
            }
        }

        public IPEndPoint EndPoint { get; set; }
        public string PlayerId { get; set; }

        public bool IsDirty { get; private set; }

        public void ResetDirty()
        {
            IsDirty = false;
        }

        public static SkirmishSlot Deserialize(NetDataReader reader)
        {
            var slot = new SkirmishSlot()
            {
                Index = reader.GetInt(),
                State = (SkirmishSlotState) reader.GetByte(),
                ColorIndex = reader.GetByte(),
                FactionIndex = reader.GetByte(),
                Team = reader.GetByte(),
                Ready = reader.GetBool()
            };

            if (slot.State == SkirmishSlotState.Human)
            {
                slot.PlayerId = reader.GetString();
                slot.PlayerName = reader.GetString();
                slot.EndPoint = reader.GetNetEndPoint();
            }

            return slot;
        }

        public static void Serialize(NetDataWriter writer, SkirmishSlot slot)
        {
            writer.Put(slot.Index);
            writer.Put((byte) slot.State);
            writer.Put(slot.ColorIndex);
            writer.Put(slot.FactionIndex);
            writer.Put(slot.Team);
            writer.Put(slot.Ready);
            if (slot.State == SkirmishSlotState.Human)
            {
                writer.Put(slot.PlayerId);
                writer.Put(slot.PlayerName);
                writer.Put(slot.EndPoint);
            }
        }
    }
}
