namespace LLGfx
{
    public struct PipelineStateDescription
    {
        public static readonly PipelineStateDescription Default = new PipelineStateDescription
        {
            RasterizerState = RasterizerStateDescription.CullBackSolid,
            DepthStencilState = DepthStencilStateDescription.Default,
            BlendState = BlendStateDescription.Opaque
        };

        public VertexDescriptor VertexDescriptor;
        public Shader VertexShader;
        public Shader PixelShader;

        public PixelFormat RenderTargetFormat;

        public RasterizerStateDescription RasterizerState;

        public DepthStencilStateDescription DepthStencilState;

        public BlendStateDescription BlendState;
    }

    public struct RasterizerStateDescription
    {
        public static readonly RasterizerStateDescription CullBackSolid = new RasterizerStateDescription(
            FillMode.Solid, true, CullMode.CullBack);

        public static readonly RasterizerStateDescription CullBackWireframe = new RasterizerStateDescription(
            FillMode.Wireframe, true, CullMode.CullBack);

        public FillMode FillMode;
        public bool IsFrontCounterClockwise;
        public CullMode CullMode;

        private RasterizerStateDescription(FillMode fillMode, bool isFrontCounterClockwise, CullMode cullMode)
        {
            FillMode = fillMode;
            IsFrontCounterClockwise = isFrontCounterClockwise;
            CullMode = cullMode;
        }
    }

    public enum CullMode
    {
        CullBack,
        CullFront,
        None
    }

    public struct DepthStencilStateDescription
    {
        public static readonly DepthStencilStateDescription Default = new DepthStencilStateDescription(
            true, true);

        public static readonly DepthStencilStateDescription DepthRead = new DepthStencilStateDescription(
            true, false);

        public static readonly DepthStencilStateDescription None = new DepthStencilStateDescription(
            false, false);

        public bool IsDepthEnabled;
        public bool IsDepthWriteEnabled;
        public Comparison DepthComparison;

        private DepthStencilStateDescription(bool isDepthEnabled, bool isDepthWriteEnabled)
        {
            IsDepthEnabled = isDepthEnabled;
            IsDepthWriteEnabled = isDepthWriteEnabled;
            DepthComparison = Comparison.LessEqual;
        }
    }

    public struct BlendStateDescription
    {
        public static readonly BlendStateDescription Opaque = new BlendStateDescription(
            false, Blend.One, Blend.Zero);

        public static readonly BlendStateDescription AlphaBlend = new BlendStateDescription(
            true, Blend.SrcAlpha, Blend.OneMinusSrcAlpha);

        public static readonly BlendStateDescription Additive = new BlendStateDescription(
            true, Blend.SrcAlpha, Blend.One);

        public bool Enabled;
        public Blend SourceBlend;
        public Blend SourceAlphaBlend;
        public Blend DestinationBlend;
        public Blend DestinationAlphaBlend;

        private BlendStateDescription(bool enabled, Blend sourceBlend, Blend destinationBlend)
        {
            Enabled = enabled;
            SourceBlend = SourceAlphaBlend = sourceBlend;
            DestinationBlend = DestinationAlphaBlend = destinationBlend;
        }
    }

    public enum FillMode
    {
        Solid,
        Wireframe
    }

    public enum Blend
    {
        Zero,
        One,
        SrcAlpha,
        OneMinusSrcAlpha,
        SrcColor,
        OneMinusSrcColor
    }
}
