using ProtoBuf;

namespace OpenSage.Network
{
    internal class LobbyProtocol
    {
        [ProtoContract]
        [ProtoInclude(101, typeof(LobbyGameMessage))]
        internal class LobbyMessage
        {
            [ProtoMember(1)]
            public string Name;
            [ProtoMember(2)]
            public bool InLobby;
        }

        internal class LobbyGameMessage : LobbyMessage
        {
            [ProtoMember(1)]
            public string Map;
            [ProtoMember(2)]
            public int Players;
        }
    }
}
