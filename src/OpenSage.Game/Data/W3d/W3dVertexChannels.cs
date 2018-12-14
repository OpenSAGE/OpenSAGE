using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dVertexChannels : uint
    {
        /// <summary>
        /// object-space location of the vertex
        /// </summary>
        Location = 1,

        /// <summary>
        /// object-space normal for the vertex
        /// </summary>
        Normal = 2,

        /// <summary>
        /// texture coordinate
        /// </summary>
        TexCoord = 4,

        /// <summary>
        /// vertex color
        /// </summary>
        Color = 8,

        /// <summary>
        /// per-vertex bone id for skins
        /// </summary>
        BoneId = 16,

        Unknown1 = 32,
        Unknown2 = 64,
    }
}
