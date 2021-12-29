using System.Collections.Generic;

namespace OpenSage.Logic
{
    internal sealed class ObjectTypeList
    {
        private readonly HashSet<string> _objectTypes;

        public string Name { get; private set; }

        public ObjectTypeList()
        {
            _objectTypes = new HashSet<string>();
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Name = reader.ReadAsciiString();

            var numObjects = (ushort)_objectTypes.Count;
            reader.ReadUInt16(ref numObjects);

            for (var i = 0; i < numObjects; i++)
            {
                _objectTypes.Add(reader.ReadAsciiString());
            }
        }
    }
}
