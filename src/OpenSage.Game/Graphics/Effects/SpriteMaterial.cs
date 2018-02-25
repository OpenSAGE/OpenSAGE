using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class SpriteMaterial : EffectMaterial
    {
        public SpriteMaterial(ContentManager contentManager, Effect effect, in OutputDescription outputDescription)
            : base(contentManager, effect)
        {
            SetSampler(contentManager.PointClampSampler);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.CullNoneSolid,
                DepthStencilStateDescription.Disabled,
                new BlendStateDescription
                {
                    AttachmentStates = new[]
                    {
                        new BlendAttachmentDescription
                        {
                            BlendEnabled = true,
                            SourceColorFactor = BlendFactor.SourceAlpha,
                            DestinationColorFactor = BlendFactor.InverseSourceAlpha,
                            ColorFunction = BlendFunction.Add,
                            SourceAlphaFactor = BlendFactor.One,
                            DestinationAlphaFactor = BlendFactor.InverseSourceAlpha,
                            AlphaFunction = BlendFunction.Add
                        }
                    }
                },
                outputDescription);
        }

        public void SetMaterialConstantsVS(DeviceBuffer value)
        {
            SetProperty("ProjectionBuffer", value);
        }

        public void SetSampler(Sampler samplerState)
        {
            SetProperty("Sampler", samplerState);
        }

        public void SetTexture(Texture texture)
        {
            SetProperty("Texture", texture);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialConstantsVS
        {
            public Matrix4x4 Projection;
        }
    }
}
