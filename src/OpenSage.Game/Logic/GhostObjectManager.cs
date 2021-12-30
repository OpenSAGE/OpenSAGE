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

            reader.PersistList(_ghostObjects, static (StatePersister persister, ref GhostObject item) =>
            {
                persister.PersistObjectID(ref item.OriginalObjectId);

                item.Load(persister);
            });
        }
    }
}
