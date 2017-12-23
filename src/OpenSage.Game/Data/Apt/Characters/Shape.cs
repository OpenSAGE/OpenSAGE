using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public class Shape : Character
    {
        public Vector4 Bounds { get; private set; }
        public uint Geometry { get; private set; }


        public static Shape Parse(BinaryReader reader)
        {
            var s = new Shape();
            s.Bounds = reader.ReadVector4();
            s.Geometry = reader.ReadUInt32();
            return s;
        }
    }
}
