namespace OpenZH.Graphics
{
    public sealed partial class GraphicsPipelineStateDescriptor
    {
        public GraphicsPipelineStateDescriptor()
        {
            PlatformConstruct();
        }

        public void SetPixelShader(Shader shader)
        {
            PlatformSetPixelShader(shader);
        }

        public void SetRenderTargetFormat(PixelFormat format)
        {
            PlatformSetRenderTargetFormat(format);
        }

        public void SetRootSignature(RootSignature rootSignature)
        {
            PlatformSetRootSignature(rootSignature);
        }

        public void SetVertexDescriptor(VertexDescriptor vertexDescriptor)
        {
            PlatformSetVertexDescriptor(vertexDescriptor);
        }

        public void SetVertexShader(Shader shader)
        {
            PlatformSetVertexShader(shader);
        }
    }
}
