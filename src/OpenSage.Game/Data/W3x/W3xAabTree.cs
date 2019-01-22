using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.W3x
{
    public sealed class W3xAabTree
    {
        public uint[] PolyIndices { get; private set; }
        public Node[] Nodes { get; private set; }

        internal static W3xAabTree Parse(BinaryReader reader)
        {
            var result = new W3xAabTree();

            result.PolyIndices = reader.ReadArrayAtOffset(() => reader.ReadUInt32());
            result.Nodes = reader.ReadArrayAtOffset(() => Node.Parse(reader));

            return result;
        }
    }

    public sealed class Child
    {
        public uint Front { get; private set; }
        public uint Back { get; private set; }
    }

    public sealed class Poly
    {
        public uint Begin { get; private set; }
        public uint Count { get; private set; }
    }

    public sealed class Node
    {
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public NodeChildren Children { get; private set; }
        public NodePolys Polys { get; private set; }

        internal static Node Parse(BinaryReader reader)
        {
            return new Node
            {
                Min = reader.ReadVector3(),
                Max = reader.ReadVector3(),
                Children = reader.ReadAtOffset(() => NodeChildren.Parse(reader)),
                Polys = reader.ReadAtOffset(() => NodePolys.Parse(reader))
            };
        }
    }

    public struct NodeChildren
    {
        public uint Front { get; private set; }
        public uint Back { get; private set; }

        internal static NodeChildren Parse(BinaryReader reader)
        {
            return new NodeChildren
            {
                Front = reader.ReadUInt32(),
                Back = reader.ReadUInt32()
            };
        }
    }

    public struct NodePolys
    {
        public uint Begin { get; private set; }
        public uint Count { get; private set; }

        internal static NodePolys Parse(BinaryReader reader)
        {
            return new NodePolys
            {
                Begin = reader.ReadUInt32(),
                Count = reader.ReadUInt32()
            };
        }
    }
}
