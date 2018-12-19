using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// Vertex Influences. For "skins" each vertex can be associated with a different bone.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dVertexInfluence
    {
        public ushort BoneIndex;

        public ushort Unknown1;
        public ushort Unknown2;
        public ushort Unknown3;

        internal static W3dVertexInfluence Parse(BinaryReader reader)
        {
            var result = new W3dVertexInfluence
            {
                BoneIndex = reader.ReadUInt16(),
                Unknown1 = reader.ReadUInt16(),
                Unknown2 = reader.ReadUInt16(),
                Unknown3 = reader.ReadUInt16()
            };

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(BoneIndex);

            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
        }
    }
}
