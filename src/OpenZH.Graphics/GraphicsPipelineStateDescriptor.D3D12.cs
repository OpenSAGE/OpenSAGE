using OpenZH.Graphics.Platforms.Direct3D12;
using SharpDX.Direct3D12;

namespace OpenZH.Graphics
{
    partial class GraphicsPipelineStateDescriptor
    {
        internal GraphicsPipelineStateDescription DeviceDescription { get; private set; }
        internal VertexDescriptor VertexDescriptor { get; private set; } // Stores vertex stride.

        private void PlatformConstruct()
        {
            DeviceDescription = new GraphicsPipelineStateDescription
            {
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                RasterizerState = RasterizerStateDescription.Default(),
                BlendState = BlendStateDescription.Default(),
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                DepthStencilState = new DepthStencilStateDescription
                {
                    IsDepthEnabled = true,
                    IsStencilEnabled = false
                },
                SampleMask = int.MaxValue,
                RenderTargetCount = 1,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                Flags = PipelineStateFlags.None,
                StreamOutput = new StreamOutputDescription()
            };
        }

        private void PlatformSetPixelShader(Shader shader)
        {
            DeviceDescription.PixelShader = shader.DeviceBytecode;
        }

        private void PlatformSetRenderTargetFormat(PixelFormat format)
        {
            DeviceDescription.RenderTargetFormats[0] = format.ToDxgiFormat();
        }

        private void PlatformSetRootSignature(RootSignature rootSignature)
        {
            DeviceDescription.RootSignature = rootSignature.DeviceRootSignature;
        }

        private void PlatformSetVertexDescriptor(VertexDescriptor vertexDescriptor)
        {
            DeviceDescription.InputLayout = vertexDescriptor.DeviceInputLayoutDescription;
            VertexDescriptor = vertexDescriptor;
        }

        private void PlatformSetVertexShader(Shader shader)
        {
            DeviceDescription.VertexShader = shader.DeviceBytecode;
        }
    }
}
