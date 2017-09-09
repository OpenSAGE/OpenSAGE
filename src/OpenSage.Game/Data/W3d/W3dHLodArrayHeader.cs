using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLodArrayHeader
    {
        public uint ModelCount { get; private set; }

        /// <summary>
        /// If model is bigger than this, switch to higher LOD.
        /// </summary>
        public float MaxScreenSize { get; private set; }

        public static W3dHLodArrayHeader Parse(BinaryReader reader)
        {
            return new W3dHLodArrayHeader
            {
                ModelCount = reader.ReadUInt32(),
                MaxScreenSize = reader.ReadSingle()
            };
        }
    }
}
