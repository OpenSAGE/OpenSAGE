using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class GhostObjectManager
    {
        private readonly List<GhostObject> _ghostObjects = new();
        private uint _unknown1;

        internal void Load(StatePersister reader, GameLogic gameLogic, Game game)
        {
            reader.ReadVersion(1);
            reader.ReadVersion(1);

            reader.ReadUInt32(ref _unknown1);

            var ghostObjectCount = (ushort)_ghostObjects.Count;
            reader.ReadUInt16(ref ghostObjectCount);

            for (var i = 0; i < ghostObjectCount; i++)
            {
                var ghostObject = new GhostObject();

                reader.ReadObjectID(ref ghostObject.OriginalObjectId);
                
                ghostObject.Load(reader, gameLogic, game);

                _ghostObjects.Add(ghostObject);
            }
        }
    }
}
