using ProtoBuf;

namespace OpenSage.Network
{
    internal class LobbyProtocol
    {
        [ProtoContract]
        internal class LobbyMessage
        {
            [ProtoMember(1)]
            public string Name;
            [ProtoMember(2)]
            public bool IsHosting;
        }

    }
}
