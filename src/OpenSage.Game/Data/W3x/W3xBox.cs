using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.W3x
{
    public sealed class W3xBox : W3xRenderObject
    {
        public Vector3 Center { get; private set; }
        public Vector3 Extent { get; private set; }
        public bool JoypadPickingOnly { get; private set; }

        public static W3xBox Parse(BinaryReader reader)
        {
            return new W3xBox
            {
                Center = reader.ReadVector3(),
                Extent = reader.ReadVector3(),
                JoypadPickingOnly = reader.ReadBooleanUInt32Checked()
            };
        }
    }
}
