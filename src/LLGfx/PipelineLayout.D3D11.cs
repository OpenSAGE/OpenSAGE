using System;
using LLGfx.Util;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;

namespace LLGfx
{
    partial class PipelineLayout
    {
        private SamplerState[] _samplerStates;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, ref PipelineLayoutDescription description)
        {
            _samplerStates = new SamplerState[description.StaticSamplerStates.Length];

            for (var i = 0; i < description.StaticSamplerStates.Length; i++)
            {
                var staticSamplerState = description.StaticSamplerStates[i];

                _samplerStates[i] = AddDisposable(new SamplerState(
                    graphicsDevice.Device, 
                    new D3D11.SamplerStateDescription
                    {
                        Filter = staticSamplerState.SamplerStateDescription.Filter.ToFilter(),
                        AddressU = staticSamplerState.SamplerStateDescription.AddressU.ToTextureAddressMode(),
                        AddressV = staticSamplerState.SamplerStateDescription.AddressV.ToTextureAddressMode(),
                        AddressW = TextureAddressMode.Clamp,
                        ComparisonFunction = D3D11.Comparison.Always,
                        MinimumLod = 0,
                        MaximumLod = float.MaxValue,
                        MaximumAnisotropy = staticSamplerState.SamplerStateDescription.MaxAnisotropy
                    }));
            }
        }

        internal void Apply(DeviceContext context)
        {
            for (var i = 0; i < Description.StaticSamplerStates.Length; i++)
            { 
                ref var staticSamplerState = ref Description.StaticSamplerStates[i];

                switch (staticSamplerState.Visibility)
                {
                    case ShaderStageVisibility.Vertex:
                        context.VertexShader.SetSampler(staticSamplerState.ShaderRegister, _samplerStates[i]);
                        break;

                    case ShaderStageVisibility.Pixel:
                        context.PixelShader.SetSampler(staticSamplerState.ShaderRegister, _samplerStates[i]);
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
