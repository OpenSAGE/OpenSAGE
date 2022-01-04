using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class GhostObjectManager
    {
        private readonly List<GhostObject> _ghostObjects = new();
        private uint _unknown1;

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);
            reader.PersistVersion(1);

            reader.PersistUInt32("Unknown1", ref _unknown1);

            reader.PersistList("GhostObjects", _ghostObjects, static (StatePersister persister, ref GhostObject item) =>
            {
                persister.BeginObject();

                item ??= new GhostObject();

                persister.PersistObjectID("OriginalObjectId", ref item.OriginalObjectId);
                persister.PersistObject("Value", item);

                persister.EndObject();
            });
        }
    }
}
