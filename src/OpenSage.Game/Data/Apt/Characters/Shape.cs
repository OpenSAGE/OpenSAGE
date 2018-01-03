using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Shape : Character
    {
        public Vector4 Bounds { get; private set; }
        public uint Geometry { get; private set; }

        public static Shape Parse(BinaryReader reader)
        {
            var shape = new Shape();
            shape.Bounds = reader.ReadVector4();
            shape.Geometry = reader.ReadUInt32();
            return shape;
        }
    }
}
