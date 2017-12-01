using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Effects
{
    public sealed class MeshEffect : Effect
    {
        public MeshEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "FixedFunctionVS", 
                  "FixedFunctionPS",
                  MeshVertex.VertexDescriptor)
        {
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SkinningConstants
    {
        public bool SkinningEnabled;
        public uint NumBones;
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

        public void SetMaterialConstants(Buffer<MaterialConstants> materialConstants)
        {
            SetProperty("MaterialConstants", materialConstants);
        }
    }
}
