using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class ScriptGroup : ScriptHierarchyNode
    {
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsSubroutine { get; private set; }
        public ScriptHierarchyNode[] ChildNodes { get; private set; }

        public static ScriptGroup Parse(BinaryReader reader, string[] assetStrings)
        {
            var unknown1 = reader.ReadUInt16();
            if (unknown1 != 2)
            {
                throw new InvalidDataException();
            }

            var dataSize = reader.ReadUInt32();
            var startPosition = reader.BaseStream.Position;

            var name = reader.ReadUInt16PrefixedAsciiString();
            var isActive = reader.ReadBoolean();
            var isSubroutine = reader.ReadBoolean();

            var endPosition = dataSize + startPosition;

            var scriptHierarchyNodes = ParseNodes(reader, endPosition, assetStrings);

            if (endPosition != reader.BaseStream.Position)
            {
                throw new InvalidDataException();
            }

            return new ScriptGroup
            {
                Name = name,
                IsActive = isActive,
                IsSubroutine = isSubroutine,
                ChildNodes = scriptHierarchyNodes
            };
        }
    }
}
