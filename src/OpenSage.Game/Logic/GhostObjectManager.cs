using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class GhostObjectManager : IPersistableObject
    {
        private readonly List<GhostObject> _ghostObjects = new();
        private uint _unknown1;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            reader.PersistVersion(1);
            reader.EndObject();

            reader.PersistUInt32(ref _unknown1);

            reader.PersistList(
                _ghostObjects,
                static (StatePersister persister, ref GhostObject item) =>
                {
                    persister.BeginObject();

                    item ??= new GhostObject();

                    persister.PersistObjectID(ref item.OriginalObjectId);
                    persister.PersistObject(item, "Value");

                    persister.EndObject();
                });
        }
    }
}
