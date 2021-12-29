using System.Collections.Generic;

namespace OpenSage.Logic
{
    internal sealed class ObjectTypeList
    {
        private readonly HashSet<string> _objectTypes;

        public string Name;

        public ObjectTypeList()
        {
            _objectTypes = new HashSet<string>();
        }

        internal void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            reader.ReadAsciiString(ref Name);

            var numObjects = (ushort)_objectTypes.Count;
            reader.ReadUInt16(ref numObjects);

            for (var i = 0; i < numObjects; i++)
            {
                var objectType = "";
                reader.ReadAsciiString(ref objectType);
                _objectTypes.Add(objectType);
            }
        }
    }
}
