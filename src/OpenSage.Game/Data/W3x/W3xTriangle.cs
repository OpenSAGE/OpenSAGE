using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3x
{
    public struct W3xTriangle
    {
        public uint VIndex0;
        public uint VIndex1;
        public uint VIndex2;

        public Vector3 Normal;

        public float Distance;

        internal static W3xTriangle Parse(BinaryReader reader)
        {
            var vertexCount = reader.ReadUInt32();
            if (vertexCount != 3)
            {
                throw new InvalidDataException();
            }

            var result = new W3xTriangle();

            reader.ReadAtOffset(() =>
            {
                result.VIndex0 = reader.ReadUInt32();
                result.VIndex1 = reader.ReadUInt32();
                result.VIndex2 = reader.ReadUInt32();
            });

            result.Normal = reader.ReadVector3();
            result.Distance = reader.ReadSingle();

            return result;
        }
    }
}
