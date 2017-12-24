using System.IO;
using System.Numerics;
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

    public sealed class Button : Character
    {
        public Vector4 Bounds { get; private set; }
        public IndexedTriangle[] Triangles { get; private set; }
        public Vector2[] Vertices { get; private set; }

        public static Button Parse(BinaryReader reader)
        {
            var button = new Button();

            var unknown = reader.ReadUInt32();
            button.Bounds = reader.ReadVector4();
            var tc = reader.ReadUInt32();
            var vc = reader.ReadUInt32();
            button.Vertices = reader.ReadFixedSizeArrayAtOffset<Vector2>(() => reader.ReadVector2(), vc);
            button.Triangles = reader.ReadFixedSizeArrayAtOffset<IndexedTriangle>(() => reader.ReadIndexedTri(), tc);

            //TODO: read actionscript related stuff and buttonrecords
            return button;
        }
    }
}
