using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class GhostObjectManager
    {
        internal void Load(SaveFileReader reader, GameLogic gameLogic, Game game)
        {
            reader.ReadVersion(1);
            reader.ReadVersion(1);

            var unknown1 = reader.ReadUInt32();

            var ghostObjectCount = reader.ReadUInt16();

            for (var i = 0; i < ghostObjectCount; i++)
            {
                var objectId = reader.ReadObjectID();
                var gameObject = gameLogic.GetObjectById(objectId);

                var ghostObject = new GhostObject(); // TODO
                ghostObject.Load(reader, gameLogic, game);
            }
        }
    }
}
