using OpenSage.Data.W3d;

namespace OpenSage.Graphics
{
    // One ModelMeshPart for each unique shader in a W3D_CHUNK_MATERIAL_PASS.
    public sealed class ModelMeshPart
    {
        //public W3dShader Shader { get; }

        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public bool AlphaTest { get; }

        internal ModelMeshPart(uint startIndex, uint indexCount, W3dShader shader)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            AlphaTest = shader.AlphaTest == W3dShaderAlphaTest.Enable;
        }
    }
}
