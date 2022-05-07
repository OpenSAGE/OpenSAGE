using System.Collections.Generic;

namespace OpenSage.Logic
{
    internal sealed class ObjectTypeList : IPersistableObject
    {
        private readonly HashSet<string> _objectTypes;

        public string Name;

        public ObjectTypeList()
        {
            _objectTypes = new HashSet<string>();
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistAsciiString(ref Name);

            reader.PersistHashSet(
                _objectTypes,
                static (StatePersister persister, ref string item) =>
                {
                    persister.PersistAsciiStringValue(ref item);
                });
        }
    }
}
