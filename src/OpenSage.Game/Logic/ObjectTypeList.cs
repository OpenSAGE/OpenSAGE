using System.Collections.Generic;
using OpenSage.Data.Sav;

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

            var numObjects = reader.ReadUInt16();
            for (var j = 0; j < numObjects; j++)
            {
                _objectTypes.Add(reader.ReadAsciiString());
            }
        }
    }
}
