using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTextureInfo
    {
        /// <summary>
        /// Flags for this texture
        /// </summary>
        public W3dTextureFlags Attributes { get; private set; }

        /// <summary>
        /// animation logic
        /// </summary>
        public W3dTextureAnimation AnimationType { get; private set; }

        /// <summary>
        /// Number of frames (1 if not animated)
        /// </summary>
        public uint FrameCount { get; private set; }

        /// <summary>
        /// Frame rate, frames per second in floating point
        /// </summary>
        public float FrameRate { get; private set; }

        public static W3dTextureInfo Parse(BinaryReader reader)
        {
            return new W3dTextureInfo
            {
                Attributes = (W3dTextureFlags) reader.ReadUInt16(),
                AnimationType = (W3dTextureAnimation) reader.ReadUInt16(),
                FrameCount = reader.ReadUInt32(),
                FrameRate = reader.ReadSingle(),
            };
        }
    }
}
