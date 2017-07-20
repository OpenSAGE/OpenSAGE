using Metal;
using OpenZH.Graphics.Platforms.Metal;

namespace OpenZH.Graphics
{
    partial class GraphicsPipelineStateDescriptor
    {
        internal MTLRenderPipelineDescriptor DeviceDescriptor { get; private set; }

        private void PlatformConstruct()
        {
            DeviceDescriptor = new MTLRenderPipelineDescriptor
            {
                DepthAttachmentPixelFormat = MTLPixelFormat.Depth32Float
            };
        }

        private void PlatformSetPixelShader(Shader shader)
        {
            DeviceDescriptor.FragmentFunction = shader.DeviceFunction;
        }

        private void PlatformSetRenderTargetFormat(PixelFormat format)
        {
            DeviceDescriptor.ColorAttachments[0].PixelFormat = format.ToMTLPixelFormat();
        }

        private void PlatformSetRootSignature(RootSignature rootSignature) { }

        private void PlatformSetVertexDescriptor(VertexDescriptor vertexDescriptor)
        {
            DeviceDescriptor.VertexDescriptor = vertexDescriptor.DeviceVertexDescriptor;
        }

        private void PlatformSetVertexShader(Shader shader)
        {
            DeviceDescriptor.VertexFunction = shader.DeviceFunction;
        }
    }
}
