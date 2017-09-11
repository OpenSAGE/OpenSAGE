namespace LLGfx
{
    public struct PipelineStateDescription
    {
        public static PipelineStateDescription Default()
        {
            return new PipelineStateDescription
            {
                IsFrontCounterClockwise = true,
                IsDepthEnabled = true,
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

        public Shader PixelShader;

        public bool IsFrontCounterClockwise;
        public bool TwoSided;

        public PixelFormat RenderTargetFormat;

        public bool IsDepthEnabled;
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
