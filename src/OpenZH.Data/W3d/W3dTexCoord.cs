using System.IO;
using System.Runtime.InteropServices;

namespace OpenZH.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dTexCoord
    {
        public const int SizeInBytes = sizeof(float) * 2;

        public float U;
        public float V;

        public static W3dTexCoord Parse(BinaryReader reader)
        {
            return new W3dTexCoord
            {
                U = reader.ReadSingle(),
                V = reader.ReadSingle()
            };
        }
    }
}
