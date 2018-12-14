using System.IO;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// The VertexMaterial defines parameters which control the calculation of the primary
    /// and secondary gradients. The shader defines how those gradients are combined with
    /// the texel and the frame buffer contents.
    /// </summary>
    public sealed class W3dMaterialInfo
    {
        /// <summary>
        /// How many material passes this render object uses
        /// </summary>
        public uint PassCount { get; private set; }

        /// <summary>
        /// How many vertex materials are used
        /// </summary>
        public uint VertexMaterialCount { get; private set; }

        /// <summary>
        /// How many shaders are used
        /// </summary>
        public uint ShaderCount { get; private set; }

        /// <summary>
        /// How many textures are used
        /// </summary>
        public uint TextureCount { get; private set; }

        internal static W3dMaterialInfo Parse(BinaryReader reader)
        {
            return new W3dMaterialInfo
            {
                PassCount = reader.ReadUInt32(),
                VertexMaterialCount = reader.ReadUInt32(),
                ShaderCount = reader.ReadUInt32(),
                TextureCount = reader.ReadUInt32()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(PassCount);
            writer.Write(VertexMaterialCount);
            writer.Write(ShaderCount);
            writer.Write(TextureCount);
        }
    }
}
