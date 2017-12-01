using LLGfx;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Effects
{
    public sealed class MeshEffect : Effect, IEffectLights
    {
        LightingType IEffectLights.LightingType => LightingType.Object;

        public MeshEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "FixedFunctionVS", 
                  "FixedFunctionPS",
                  MeshVertex.VertexDescriptor)
        {
        }

        public void SetSkinningBuffer(Buffer<Matrix4x3> skinningBuffer)
        {
            SetValue("SkinningBuffer", skinningBuffer);
        }

        public void SetSkinningEnabled(bool enabled)
        {
            SetConstantBufferField("SkinningConstants", "SkinningEnabled", enabled);
        }

        public void SetNumBones(uint numBones)
        {
            SetConstantBufferField("SkinningConstants", "NumBones", numBones);
        }
    }

    public sealed class MeshMaterial : EffectMaterial
    {
        public MeshMaterial(MeshEffect effect)
            : base(effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.SamplerAnisotropicWrap);
        }

        public void SetTexture0(Texture texture)
        {
            SetProperty("Texture0", texture);
        }

        public void SetTexture1(Texture texture)
        {
            SetProperty("Texture1", texture);
        }

        public void SetVertexMaterial(ref VertexMaterial vertexMaterial)
        {
            SetProperty("MaterialConstants", "Material", ref vertexMaterial);
        }

        public void SetShadingConfiguration(ref ShadingConfiguration shadingConfiguration)
        {
            SetProperty("MaterialConstants", "Shading", ref shadingConfiguration);
        }

        public void SetNumTextureStages(uint numTextureStages)
        {
            SetProperty("MaterialConstants", "NumTextureStages", numTextureStages);
        }
    }
}
