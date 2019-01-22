using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.FileFormats.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dRingScaleKeyFrame
    {
        public byte Version;
        public byte Size;
        public float X;
        public float Y;
        public float Position;

        internal static W3dRingScaleKeyFrame Parse(BinaryReader reader)
        {
            return new W3dRingScaleKeyFrame
            {
                Version = reader.ReadByte(),
                Size = reader.ReadByte(),
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Position = reader.ReadSingle()
            };
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Size);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Position);
        }
    }
}
