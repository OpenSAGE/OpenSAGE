﻿using System.IO;
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

        internal static W3dVertexInfluence Parse(BinaryReader reader)
        {
            var result = new W3dVertexInfluence
            {
                BoneIndex = reader.ReadUInt16()
            };

            reader.ReadBytes(6); // Padding

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(BoneIndex);

            writer.BaseStream.Seek(6, SeekOrigin.Current); // Padding
        }
    }
}
