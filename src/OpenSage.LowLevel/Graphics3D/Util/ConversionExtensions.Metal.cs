using System;
using Metal;

namespace OpenSage.LowLevel.Graphics3D.Util
{
    internal static class ConversionExtensions
    {
        public static MTLVertexStepFunction ToMTLVertexStepFunction(this InputClassification value)
        {
            switch (value)
            {
                case InputClassification.PerVertexData:
                    return MTLVertexStepFunction.PerVertex;

                case InputClassification.PerInstanceData:
                    return MTLVertexStepFunction.PerInstance;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLVertexFormat ToMTLVertexFormat(this VertexFormat value)
        {
            switch (value)
            {
                case VertexFormat.Float:
                    return MTLVertexFormat.Float;

                case VertexFormat.Float2:
                    return MTLVertexFormat.Float2;

                case VertexFormat.Float3:
                    return MTLVertexFormat.Float3;

                case VertexFormat.Float4:
                    return MTLVertexFormat.Float4;

                case VertexFormat.UInt:
                    return MTLVertexFormat.UInt;

                case VertexFormat.UInt2:
                    return MTLVertexFormat.UInt2;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ShaderType ToShaderType(this MTLFunctionType value)
        {
            switch (value)
            {
                case MTLFunctionType.Vertex:
                    return ShaderType.VertexShader;

                case MTLFunctionType.Fragment:
                    return ShaderType.PixelShader;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void ToMTLSamplerFilters(
            this SamplerFilter filter,
            out MTLSamplerMinMagFilter minFilter,
            out MTLSamplerMinMagFilter magFilter,
            out MTLSamplerMipFilter mipFilter)
        {
            switch (filter)
            {
                case SamplerFilter.MinMagMipPoint:
                    minFilter = magFilter = MTLSamplerMinMagFilter.Nearest;
                    mipFilter = MTLSamplerMipFilter.Nearest;
                    break;

                case SamplerFilter.MinMagMipLinear:
                    minFilter = magFilter = MTLSamplerMinMagFilter.Linear;
                    mipFilter = MTLSamplerMipFilter.Linear;
                    break;

                case SamplerFilter.Anisotropic:
                    minFilter = magFilter = MTLSamplerMinMagFilter.Linear;
                    mipFilter = MTLSamplerMipFilter.Linear;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }

        public static MTLSamplerAddressMode ToMTLSamplerAddressMode(this SamplerAddressMode value)
        {
            switch (value)
            {
                case SamplerAddressMode.Clamp:
                    return MTLSamplerAddressMode.ClampToEdge;

                case SamplerAddressMode.Wrap:
                    return MTLSamplerAddressMode.Repeat;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public static MTLPrimitiveType ToMTLPrimitiveType(this PrimitiveType value)
        {
            switch (value)
            {
                case PrimitiveType.TriangleList:
                    return MTLPrimitiveType.Triangle;

                    
                case PrimitiveType.LineList:
                    return MTLPrimitiveType.Line;
                    
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public static MTLPrimitiveTopologyClass ToMTLPrimitiveTopologyClass(this PrimitiveType value)
        {
            switch (value)
            {
                case PrimitiveType.TriangleList:
                    return MTLPrimitiveTopologyClass.Triangle;


                case PrimitiveType.LineList:
                    return MTLPrimitiveTopologyClass.Line;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public static MTLViewport ToMTLViewport(this Viewport value)
        {
            return new MTLViewport(value.X, value.Y, value.Width, value.Height, value.MinDepth, value.MaxDepth);
        }

        public static MTLClearColor ToMTLClearColor(this ColorRgbaF value)
        {
            return new MTLClearColor(value.R, value.G, value.B, value.A);
        }

        public static MTLLoadAction ToMTLLoadAction(this LoadAction value)
        {
            switch (value)
            {
                case LoadAction.DontCare:
                    return MTLLoadAction.DontCare;

                case LoadAction.Clear:
                    return MTLLoadAction.Clear;

                case LoadAction.Load:
                    return MTLLoadAction.Load;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLTextureUsage ToMTLTextureUsage(this TextureBindFlags value)
        {
            switch (value)
            {
                case TextureBindFlags.None:
                    return MTLTextureUsage.Unknown;

                case TextureBindFlags.ShaderResource:
                    return MTLTextureUsage.ShaderRead;

                case TextureBindFlags.RenderTarget:
                    return MTLTextureUsage.RenderTarget;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLPixelFormat ToMTLPixelFormat(this PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.Bc1:
                    return MTLPixelFormat.BC1RGBA;
                    
                case PixelFormat.Bc2:
                    return MTLPixelFormat.BC2RGBA;

                case PixelFormat.Bc3:
                    return MTLPixelFormat.BC3RGBA;

                case PixelFormat.Bgra8UNorm:
                    return MTLPixelFormat.BGRA8Unorm;

                case PixelFormat.Rgba8UNorm:
                    return MTLPixelFormat.RGBA8Unorm;

                case PixelFormat.R32UInt:
                    return MTLPixelFormat.R32Uint;

                case PixelFormat.Rgba32UInt:
                    return MTLPixelFormat.RGBA32Uint;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLTriangleFillMode ToMTLTriangleFillMode(this FillMode value)
        {
            switch (value)
            {
                case FillMode.Solid:
                    return MTLTriangleFillMode.Fill;

                case FillMode.Wireframe:
                    return MTLTriangleFillMode.Lines;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLCullMode ToMTLCullMode(this CullMode value)
        {
            switch (value)
            {
                case CullMode.CullBack:
                    return MTLCullMode.Back;

                case CullMode.CullFront:
                    return MTLCullMode.Front;

                case CullMode.None:
                    return MTLCullMode.None;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLCompareFunction ToMTLCompareFunction(this Comparison value)
        {
            switch (value)
            {
                case Comparison.Never:
                    return MTLCompareFunction.Never;

                case Comparison.Less:
                    return MTLCompareFunction.Less;

                case Comparison.Equal:
                    return MTLCompareFunction.Equal;

                case Comparison.LessEqual:
                    return MTLCompareFunction.LessEqual;

                case Comparison.Greater:
                    return MTLCompareFunction.Greater;

                case Comparison.NotEqual:
                    return MTLCompareFunction.NotEqual;

                case Comparison.GreaterEqual:
                    return MTLCompareFunction.GreaterEqual;

                case Comparison.Always:
                    return MTLCompareFunction.Always;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLBlendFactor ToMTLBlendFactor(this Blend value)
        {
            switch (value)
            {
                case Blend.Zero:
                    return MTLBlendFactor.Zero;
                    
                case Blend.One:
                    return MTLBlendFactor.One;
                    
                case Blend.SrcAlpha:
                    return MTLBlendFactor.SourceAlpha;

                case Blend.OneMinusSrcAlpha:
                    return MTLBlendFactor.OneMinusSourceAlpha;
                    
                case Blend.SrcColor:
                    return MTLBlendFactor.SourceColor;
                    
                case Blend.OneMinusSrcColor:
                    return MTLBlendFactor.OneMinusSourceColor;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
