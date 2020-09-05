using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats;

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

        internal void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();

            Name = reader.ReadBytePrefixedAsciiString();

            var numObjects = reader.ReadUInt16();
            for (var j = 0; j < numObjects; j++)
            {
                _objectTypes.Add(reader.ReadBytePrefixedAsciiString());
            }
        }
    }
}
