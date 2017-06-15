using System.IO;

namespace OpenZH.Data.W3d
{
    public sealed class W3dVertexMaterial
    {
        public W3dVertexMaterialFlags Attributes { get; private set; }

        public W3dRgb Ambient { get; private set; }
        public W3dRgb Diffuse { get; private set; }
        public W3dRgb Specular { get; private set; }
        public W3dRgb Emissive { get; private set; }

        /// <summary>
        /// how tight the specular highlight will be, 1 - 1000 (default = 1)
        /// </summary>
        public float Shininess { get; private set; }

        /// <summary>
        /// how opaque the material is, 0.0 = invisible, 1.0 = fully opaque (default = 1)
        /// </summary>
        public float Opacity { get; private set; }

        /// <summary>
        /// how much light passes through the material. (default = 0)
        /// </summary>
        public float Translucency { get; private set; }

        public static W3dVertexMaterial Parse(BinaryReader reader)
        {
            return new W3dVertexMaterial
            {
                Attributes = (W3dVertexMaterialFlags) reader.ReadUInt32(),

                Ambient = W3dRgb.Parse(reader),
                Diffuse = W3dRgb.Parse(reader),
                Specular = W3dRgb.Parse(reader),
                Emissive = W3dRgb.Parse(reader),

                Shininess = reader.ReadSingle(),
                Opacity = reader.ReadSingle(),
                Translucency = reader.ReadSingle()
            };
        }
    }
}
