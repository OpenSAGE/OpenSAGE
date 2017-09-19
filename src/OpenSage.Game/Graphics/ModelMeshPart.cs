using LLGfx;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics
{
    // One ModelMeshPart for each unique shader in a W3D_CHUNK_MATERIAL_PASS.
    public sealed class ModelMeshPart
    {
        public uint StartIndex { get; }
        public uint IndexCount { get; }

        public bool AlphaTest { get; }
        public bool Texturing { get; }

        public EffectPipelineStateHandle PipelineStateHandle { get; }

        internal ModelMeshPart(
            uint startIndex, 
            uint indexCount, 
            W3dMesh w3dMesh, 
            W3dShader w3dShader)
        {
            StartIndex = startIndex;
            IndexCount = indexCount;

            AlphaTest = w3dShader.AlphaTest == W3dShaderAlphaTest.Enable;
            Texturing = w3dShader.Texturing == W3dShaderTexturing.Enable;

            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.CullMode = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.TwoSided)
                ? CullMode.None
                : CullMode.CullBack;

            var depthState = DepthStencilStateDescription.Default;
            depthState.IsDepthEnabled = true;
            depthState.IsDepthWriteEnabled = w3dShader.DepthMask == W3dShaderDepthMask.WriteEnable;
            // TODO: DepthCompare

            var blendState = BlendStateDescription.Opaque;
            blendState.Enabled = w3dShader.SrcBlend != W3dShaderSrcBlendFunc.One 
                || w3dShader.DestBlend != W3dShaderDestBlendFunc.Zero;
            blendState.SourceBlend = w3dShader.SrcBlend.ToBlend();
            blendState.DestinationBlend = w3dShader.DestBlend.ToBlend();

            PipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                depthState,
                blendState)
                .GetHandle();
        }
    }
}
