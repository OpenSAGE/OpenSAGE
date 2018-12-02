using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class ShaderMaterial : MeshMaterial
    {
        public ShaderMaterial(ContentManager contentManager, Effect effect) 
            : base(contentManager, effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.Aniso4xSampler);
        }

        public void SetMaterialConstants(DeviceBuffer buffer)
        {
            SetProperty("MaterialConstants", buffer);
        }
    }
}
