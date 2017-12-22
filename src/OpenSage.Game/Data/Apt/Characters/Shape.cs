using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public class Shape : Character
    {
        public Vector4 Bounds { get; private set; }
        public uint Geometry { get; private set; } 


        public static Shape Parse(BinaryReader br)
        {
            var s = new Shape();
            s.Bounds = br.ReadVector4();
            s.Geometry = br.ReadUInt32();
            return s;
        }
    }
}
