using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class GhostObjectManager
    {
        private readonly List<GhostObject> _ghostObjects = new();
        private uint _unknown1;

        internal void Load(SaveFileReader reader, GameLogic gameLogic, Game game)
        {
            reader.ReadVersion(1);
            reader.ReadVersion(1);

            _unknown1 = reader.ReadUInt32();

            var ghostObjectCount = reader.ReadUInt16();

            for (var i = 0; i < ghostObjectCount; i++)
            {
                var ghostObject = new GhostObject();

                ghostObject.OriginalObjectId = reader.ReadObjectID();
                
                ghostObject.Load(reader, gameLogic, game);

                _ghostObjects.Add(ghostObject);
            }
        }
    }
}
