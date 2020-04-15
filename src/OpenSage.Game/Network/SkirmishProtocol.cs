using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace OpenSage.Network
{
    internal class SkirmishProtocol
    {
        [ProtoContract]
        internal class UpdateState
        {
            [ProtoMember(1)]
            public byte ColorIndex;
            [ProtoMember(2)]
            public byte FactionIndex;
            [ProtoMember(3)]
            public string Name;
        }
    }
}
