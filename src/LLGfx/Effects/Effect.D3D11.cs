using System;
using SharpDX.D3DCompiler;

namespace LLGfx.Effects
{
    partial class Effect
    {
        private void PlatformDoReflection(
            VertexShader vertexShader, 
            PixelShader pixelShader,
            out EffectResourceBinding[] vertexShaderResourceBindings,
            out EffectResourceBinding[] pixelShaderResourceBindings)
        {
            using (var vertexShaderReflection = new ShaderReflection(vertexShader.DeviceBytecode))
            using (var pixelShaderReflection = new ShaderReflection(pixelShader.DeviceBytecode))
            {
                vertexShaderResourceBindings = GetResourceBindings(vertexShaderReflection, EffectResourceShaderStage.VertexShader);
                pixelShaderResourceBindings = GetResourceBindings(pixelShaderReflection, EffectResourceShaderStage.PixelShader);
            }
        }

        private static EffectResourceBinding[] GetResourceBindings(
            ShaderReflection reflection,
            EffectResourceShaderStage shaderStage)
        {
            var result = new EffectResourceBinding[reflection.Description.BoundResources];

            for (var i = 0; i < result.Length; i++)
            {
                var resourceDescription = reflection.GetResourceBindingDescription(i);

                result[i] = new EffectResourceBinding(
                    resourceDescription.Name,
                    GetResourceType(resourceDescription.Type),
                    shaderStage,
                    resourceDescription.BindPoint);
            }

            return result;
        }

        private static EffectResourceType GetResourceType(ShaderInputType type)
        {
            switch (type)
            {
                case ShaderInputType.ConstantBuffer:
                    return EffectResourceType.ConstantBuffer;
                    
                case ShaderInputType.Texture:
                    return EffectResourceType.Texture;

                case ShaderInputType.Sampler:
                    return EffectResourceType.Sampler;
                    
                case ShaderInputType.Structured:
                    return EffectResourceType.StructuredBuffer;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
