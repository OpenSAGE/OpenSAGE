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
        public ushort Bone2Index;
        public ushort BoneWeight;
        public ushort Bone2Weight;

        internal static W3dVertexInfluence Parse(BinaryReader reader)
        {
            var result = new W3dVertexInfluence
            {
                BoneIndex = reader.ReadUInt16(),
                Bone2Index = reader.ReadUInt16(),
                BoneWeight = (reader.ReadUInt16()),
                Bone2Weight = (reader.ReadUInt16())
            };

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(BoneIndex);
            writer.Write(Bone2Index);
            writer.Write(BoneWeight);
            writer.Write(Bone2Weight);
        }
    }
}
