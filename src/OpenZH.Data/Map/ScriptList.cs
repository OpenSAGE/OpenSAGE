using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class ScriptList
    {
        public ScriptHierarchyNode[] ChildNodes { get; private set; }

        public static ScriptList Parse(BinaryReader reader, string[] assetStrings)
        {
            var header = reader.ReadUInt32();
            if (assetStrings[header - 1] != "ScriptList")
            {
                throw new InvalidDataException();
            }

            var unknown = reader.ReadUInt16();
            if (unknown != 1)
            {
                throw new InvalidDataException();
            }

            var dataSize = reader.ReadUInt32();
            var endPosition = reader.BaseStream.Position + dataSize;

            var scriptHierarchyNodes = ScriptHierarchyNode.ParseNodes(reader, endPosition, assetStrings);

            if (reader.BaseStream.Position != endPosition)
            {
                throw new InvalidDataException();
            }

            return new ScriptList
            {
                ChildNodes = scriptHierarchyNodes
            };
        }
    }
}
