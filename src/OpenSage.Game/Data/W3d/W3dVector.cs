using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct W3dVector
    {
        public float X;
        public float Y;
        public float Z;

        private string DebugDisplayString => $"{X} {Y} {Z}";

        public static W3dVector Parse(BinaryReader reader)
        {
            return new W3dVector
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };
        }
    }
}
