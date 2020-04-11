using ProtoBuf;

namespace OpenSage.Network
{
    internal class LobbyProtocol
    {
        [ProtoContract]
        internal struct LobbyBroadcast     {
            [ProtoMember(1)]
            public string Name;
            [ProtoMember(2)]
            public bool InLobby;
            [ProtoMember(3)]
            public bool Host;
            [ProtoMember(4)]
            public string Map;
            [ProtoMember(5)]
            public int Players;
        }
    }
}
