using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dVertexChannels : uint
    {
        /// <summary>
        /// object-space location of the vertex
        /// </summary>
        Location = 0x00000001,

        /// <summary>
        /// object-space normal for the vertex
        /// </summary>
        Normal = 0x00000002,

        /// <summary>
        /// texture coordinate
        /// </summary>
        TexCoord = 0x00000004,

        /// <summary>
        /// vertex color
        /// </summary>
        Color = 0x00000008,

        /// <summary>
        /// per-vertex bone id for skins
        /// </summary>
        BoneId = 0x00000010
    }
}
