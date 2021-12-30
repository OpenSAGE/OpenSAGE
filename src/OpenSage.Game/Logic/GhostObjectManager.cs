using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class GhostObjectManager
    {
        private readonly List<GhostObject> _ghostObjects = new();
        private uint _unknown1;

        internal void Load(StatePersister reader, GameLogic gameLogic, Game game)
        {
            reader.PersistVersion(1);
            reader.PersistVersion(1);

            reader.PersistUInt32(ref _unknown1);

            var ghostObjectCount = (ushort)_ghostObjects.Count;
            reader.PersistUInt16(ref ghostObjectCount);

            for (var i = 0; i < ghostObjectCount; i++)
            {
                var ghostObject = new GhostObject();

                reader.PersistObjectID(ref ghostObject.OriginalObjectId);
                
                ghostObject.Load(reader, gameLogic, game);

                _ghostObjects.Add(ghostObject);
            }
        }
    }
}
