using System.Net;
using LiteNetLib.Utils;

namespace OpenSage.Network
{
    public enum SkirmishSlotState : byte
    {
        Open = 0,
        Closed = 1,
        EasyArmy = 2,
        MediumArmy = 3,
        HardArmy = 4,
        Human = 5,
    }

    public class SkirmishSlot
    {
        private int _colorChosen;
        private int _startPositionChosen;
        private int _playerTemplateIndexChosen;

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
        public string PlayerName
        {
            get
            {
                return _playerName;
            }
            set
            {
                IsDirty |= _playerName != value;
                _playerName = value;
            }
        }

        private sbyte _colorIndex;
        public sbyte ColorIndex
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
        public byte FactionIndex
        {
            get
            {
                return _factionIndex;
            }
            set
            {
                IsDirty |= _factionIndex != value;
                _factionIndex = value;
            }
        }

        private sbyte _team;
        public sbyte Team
        {
            get
            {
                return _team;
            }
            set
            {
                IsDirty |= _team != value;
                _team = value;
            }
        }

        private byte _startPosition;
        public byte StartPosition
        {
            get
            {
                return _startPosition;
            }
            set
            {
                IsDirty |= _startPosition != value;
                _startPosition = value;
            }
        }

        public bool Ready { get; set; }
        public bool ReadyUpdated { get; set; }

        public IPEndPoint EndPoint { get; set; }
        public string ClientId { get; set; }

        public bool IsDirty { get; private set; }

        public void ResetDirty()
        {
            IsDirty = false;
        }

        public static SkirmishSlot Deserialize(NetDataReader reader)
        {
            var slot = new SkirmishSlot(reader.GetInt())
            {
                State = (SkirmishSlotState) reader.GetByte(),
                ColorIndex = reader.GetSByte(),
                FactionIndex = reader.GetByte(),
                Team = reader.GetSByte(),
                StartPosition = reader.GetByte(),
            };

            if (slot.State == SkirmishSlotState.Human)
            {
                slot.ClientId = reader.GetString();
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
            writer.Put(slot.StartPosition);
            if (slot.State == SkirmishSlotState.Human)
            {
                writer.Put(slot.ClientId);
                writer.Put(slot.PlayerName);
                writer.Put(slot.EndPoint);
            }
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadEnum(ref _state);
            reader.ReadUnicodeString(ref _playerName);

            ushort unknown1 = 1;
            reader.ReadUInt16(ref unknown1);
            if (unknown1 != 1)
            {
                throw new InvalidStateException();
            }

            int colorIndex = ColorIndex;
            reader.ReadInt32(ref colorIndex);
            ColorIndex = (sbyte)colorIndex;

            int startPosition = StartPosition;
            reader.ReadInt32(ref startPosition);
            StartPosition = (byte)startPosition;

            // Bit ugly... this is really an index into player templates,
            // but FactionIndex only counts playable sides... and also is 1-based.
            int factionIndex = FactionIndex;
            reader.ReadInt32(ref factionIndex);
            FactionIndex = (byte)(factionIndex - 1);

            int team = Team;
            reader.ReadInt32(ref team);
            Team = (sbyte)team;

            reader.ReadInt32(ref _colorChosen);
            reader.ReadInt32(ref _startPositionChosen);
            reader.ReadInt32(ref _playerTemplateIndexChosen);
        }
    }
}
