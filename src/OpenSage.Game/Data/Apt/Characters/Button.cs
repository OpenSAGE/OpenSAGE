using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt.Characters
{
    enum ButtonState
    {
        Idle = 0,
        OutUp = 1,
        OverUp = 2,
        OutDown = 3,
        OverDown = 4,
    }

    public class Button : Character
    {
        public Vector4 Bounds { get; private set; }
        public IndexedTriangle[] Triangles { get; private set; }
        public Vector2[] Vertices { get; private set; }

        public static Button Parse(BinaryReader br)
        {
            var b = new Button();

            var unknown = br.ReadUInt32();
            b.Bounds = br.ReadVector4();
            var tc = br.ReadUInt32();
            var vc = br.ReadUInt32();
            b.Vertices = br.ReadFixedSizeArrayAtOffset<Vector2>(() => br.ReadVector2(),vc);
            b.Triangles = br.ReadFixedSizeArrayAtOffset<IndexedTriangle>(() => br.ReadIndexedTri(), tc);

            //TODO: read actionscript related stuff and buttonrecords
            return b;
        }
    }
}
