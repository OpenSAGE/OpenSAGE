using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using D3D12 = SharpDX.Direct3D12;

namespace LLGfx.Util
{
    internal static class ConversionExtensions
    {
        public static ResourceStates ToResourceStates(this ResourceState value)
        {
            switch (value)
            {
                case ResourceState.RenderTarget:
                    return ResourceStates.RenderTarget;

                case ResourceState.Present:
                    return ResourceStates.Present;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static D3D12.InputClassification ToInputClassification(this InputClassification value)
        {
            switch (value)
            {
                case InputClassification.PerVertexData:
                    return D3D12.InputClassification.PerVertexData;

                case InputClassification.PerInstanceData:
                    return D3D12.InputClassification.PerInstanceData;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static D3D12.RasterizerStateDescription ToRasterizerStateDescription(this RasterizerStateDescription value)
        {
            var result = D3D12.RasterizerStateDescription.Default();

            result.IsFrontCounterClockwise = value.IsFrontCounterClockwise;

            result.FillMode = value.FillMode.ToFillMode();

            result.CullMode = value.CullMode.ToCullMode();

            if (value.FillMode == FillMode.Wireframe)
            {
                result.SlopeScaledDepthBias = -1;
            }

            return result;
        }

        public static D3D12.BlendStateDescription ToBlendStateDescription(this BlendStateDescription value)
        {
            var result = D3D12.BlendStateDescription.Default();

            if (value.Enabled)
            {
                result.RenderTarget[0].IsBlendEnabled = true;
                result.RenderTarget[0].SourceBlend = value.SourceBlend.ToBlendOption();
                result.RenderTarget[0].SourceAlphaBlend = value.SourceAlphaBlend.ToBlendOption();
                result.RenderTarget[0].DestinationBlend = value.DestinationBlend.ToBlendOption();
                result.RenderTarget[0].DestinationAlphaBlend = value.DestinationAlphaBlend.ToBlendOption();
            }

            return result;
        }

        public static D3D12.DepthStencilStateDescription ToDepthStencilStateDescription(this DepthStencilStateDescription value)
        {
            var result = D3D12.DepthStencilStateDescription.Default();

            result.IsDepthEnabled = value.IsDepthEnabled;

            result.DepthWriteMask = value.IsDepthWriteEnabled
                ? DepthWriteMask.All
                : DepthWriteMask.Zero;

            result.DepthComparison = value.DepthComparison.ToComparison();

            return result;
        }

        public static D3D12.Comparison ToComparison(this Comparison value)
        {
            switch (value)
            {
                case Comparison.Never:
                    return D3D12.Comparison.Never;

                case Comparison.Less:
                    return D3D12.Comparison.Less;

                case Comparison.Equal:
                    return D3D12.Comparison.Equal;

                case Comparison.LessEqual:
                    return D3D12.Comparison.LessEqual;

                case Comparison.Greater:
                    return D3D12.Comparison.Greater;

                case Comparison.NotEqual:
                    return D3D12.Comparison.NotEqual;

                case Comparison.GreaterEqual:
                    return D3D12.Comparison.GreaterEqual;

                case Comparison.Always:
                    return D3D12.Comparison.Always;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static D3D12.FillMode ToFillMode(this FillMode value)
        {
            switch (value)
            {
                case FillMode.Solid:
                    return D3D12.FillMode.Solid;

                case FillMode.Wireframe:
                    return D3D12.FillMode.Wireframe;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static D3D12.CullMode ToCullMode(this CullMode value)
        {
            switch (value)
            {
                case CullMode.CullBack:
                    return D3D12.CullMode.Back;

                case CullMode.CullFront:
                    return D3D12.CullMode.Front;

                case CullMode.None:
                    return D3D12.CullMode.None;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BlendOption ToBlendOption(this Blend value)
        {
            switch (value)
            {
                case Blend.Zero:
                    return BlendOption.Zero;

                case Blend.One:
                    return BlendOption.One;

                case Blend.SrcAlpha:
                    return BlendOption.SourceAlpha;

                case Blend.OneMinusSrcAlpha:
                    return BlendOption.InverseSourceAlpha;

                case Blend.SrcColor:
                    return BlendOption.SourceColor;

                case Blend.OneMinusSrcColor:
                    return BlendOption.InverseSourceColor;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DescriptorRangeType ToDescriptorRangeType(this ResourceType value)
        {
            switch (value)
            {
                case ResourceType.ConstantBuffer:
                    return DescriptorRangeType.ConstantBufferView;

                case ResourceType.StructuredBuffer:
                case ResourceType.Texture:
                    return DescriptorRangeType.ShaderResourceView;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Format ToDxgiFormat(this PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.Bc1:
                    return Format.BC1_UNorm;

                case PixelFormat.Bc2:
                    return Format.BC2_UNorm;

                case PixelFormat.Bc3:
                    return Format.BC3_UNorm;

                case PixelFormat.Bgra8UNorm:
                    return Format.B8G8R8A8_UNorm;

                case PixelFormat.Rgba8UNorm:
                    return Format.R8G8B8A8_UNorm;

                case PixelFormat.R32UInt:
                    return Format.R32_UInt;

                case PixelFormat.Rgba32UInt:
                    return Format.R32G32B32A32_UInt;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Format ToDxgiFormat(this VertexFormat value)
        {
            switch (value)
            {
                case VertexFormat.Float:
                    return Format.R32_Float;

                case VertexFormat.Float2:
                    return Format.R32G32_Float;

                case VertexFormat.Float3:
                    return Format.R32G32B32_Float;

                case VertexFormat.Float4:
                    return Format.R32G32B32A32_Float;

                case VertexFormat.UInt:
                    return Format.R32_UInt;

                case VertexFormat.UInt2:
                    return Format.R32G32_UInt;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Filter ToFilter(this SamplerFilter value)
        {
            switch (value)
            {
                case SamplerFilter.MinMagMipPoint:
                    return Filter.MinMagMipPoint;

                case SamplerFilter.MinMagMipLinear:
                    return Filter.MinMagMipLinear;

                case SamplerFilter.Anisotropic:
                    return Filter.Anisotropic;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public static PrimitiveTopology ToPrimitiveTopology(this PrimitiveType value)
        {
            switch (value)
            {
                case PrimitiveType.TriangleList:
                    return PrimitiveTopology.TriangleList;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static RawColor4 ToRawColor4(this ColorRgbaF value)
        {
            return new RawColor4(value.R, value.G, value.B, value.A);
        }

        public static RootParameterType ToRootParameterType(this ResourceType value)
        {
            switch (value)
            {
                case ResourceType.ConstantBuffer:
                    return RootParameterType.ConstantBufferView;

                case ResourceType.StructuredBuffer:
                case ResourceType.Texture:
                    return RootParameterType.ShaderResourceView;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ShaderVisibility ToShaderVisibility(this ShaderStageVisibility value)
        {
            switch (value)
            {
                case ShaderStageVisibility.All:
                    return ShaderVisibility.All;

                case ShaderStageVisibility.Vertex:
                    return ShaderVisibility.Vertex;

                case ShaderStageVisibility.Pixel:
                    return ShaderVisibility.Pixel;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static RawViewportF ToViewportF(this Viewport v)
        {
            return new RawViewportF
            {
                X = v.X,
                Y = v.Y,
                Width = v.Width,
                Height = v.Height,
                MinDepth = v.MinDepth,
                MaxDepth = v.MaxDepth
            };
        }
    }
}
