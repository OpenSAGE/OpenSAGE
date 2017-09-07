namespace LLGfx
{
    public struct PipelineStateDescription
    {
        public static PipelineStateDescription Default()
        {
            return new PipelineStateDescription
            {
                IsDepthWriteEnabled = true,
                Blending = new BlendDescription
                {
                    SourceBlend = Blend.One,
                    DestinationBlend = Blend.Zero
                }
            };
        }

        public PipelineLayout PipelineLayout;

        public VertexDescriptor VertexDescriptor;
        public Shader VertexShader;

        public bool TwoSided;

        public Shader PixelShader;

        public PixelFormat RenderTargetFormat;
        public bool IsDepthWriteEnabled;

        public BlendDescription Blending;
    }

    public struct BlendDescription
    {
        public bool Enabled;
        public Blend SourceBlend;
        public Blend DestinationBlend;
    }
}
