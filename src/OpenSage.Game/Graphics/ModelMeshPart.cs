using LLGfx;
using OpenSage.Data.W3d;

namespace OpenSage.Graphics
{
    // One ModelMeshPart for each unique shader in a W3D_CHUNK_MATERIAL_PASS.
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public bool AlphaTest { get; }
        public bool Texturing { get; }

        public PipelineState PipelineState { get; }

        internal ModelMeshPart(uint startIndex, uint indexCount, W3dMesh mesh, W3dShader shader, ModelRenderer modelRenderer)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            AlphaTest = shader.AlphaTest == W3dShaderAlphaTest.Enable;
            Texturing = shader.Texturing == W3dShaderTexturing.Enable;

            PipelineState = modelRenderer.GetPipelineState(mesh, shader);
        }
    }
}
