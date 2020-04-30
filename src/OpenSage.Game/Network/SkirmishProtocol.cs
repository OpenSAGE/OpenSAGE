using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using static OpenSage.Network.SkirmishManager.SkirmishSlot;

namespace OpenSage.Network
{
    internal class SkirmishProtocol
    {
        [ProtoContract]
        internal class UpdateState
        {
            [ProtoMember(1)]
            public SkirmishSlot Slot;
        }

        [ProtoContract]
        internal class SkirmishSlot
        {
            [ProtoMember(1)]
            public SlotType Type;
            [ProtoMember(2)]
            public byte ColorIndex;
            [ProtoMember(3)]
            public byte FactionIndex;
            [ProtoMember(4)]
            public TeamType Team;
            [ProtoMember(5)]
            public string Name;
            [ProtoMember(6)]
            public bool Ready;
        }

        [ProtoContract]
        internal class SkirmishState
        {
            [ProtoMember(1)]
            public SkirmishSlot[] Slots;

            [ProtoMember(2)]
            public string Map;
        }
    }
}
