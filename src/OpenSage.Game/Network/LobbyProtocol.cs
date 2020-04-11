using ProtoBuf;

namespace OpenSage.Network
{
    internal class LobbyProtocol
    {
        [ProtoContract]
        internal struct LobbyBroadcast
        {
            [ProtoMember(1)]
            public string Name;
            [ProtoMember(2)]
            public string Map;
            [ProtoMember(3)]
            public int Players;
        }
    }
}
