using System;
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
        public ushort BoneIndex0;
        public ushort BoneIndex1;
        public ushort BoneWeight0;
        public ushort BoneWeight1;

        internal static W3dVertexInfluence Parse(BinaryReader reader)
        {
            var result = new W3dVertexInfluence
            {
                BoneIndex0 = reader.ReadUInt16(),
                BoneIndex1 = reader.ReadUInt16(),
                BoneWeight0 = reader.ReadUInt16(),
                BoneWeight1 = reader.ReadUInt16()
            };

            if (result.BoneWeight0 == 0)
            {
                throw new Exception();
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(BoneIndex0);
            writer.Write(BoneIndex1);
            writer.Write(BoneWeight0);
            writer.Write(BoneWeight1);
        }
    }
}
