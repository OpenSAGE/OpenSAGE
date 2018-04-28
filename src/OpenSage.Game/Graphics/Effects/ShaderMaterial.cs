using OpenSage.Content;

namespace OpenSage.Graphics.Effects
{
    public sealed class ShaderMaterial : MeshMaterial
    {
        public ShaderMaterial(ContentManager contentManager, Effect effect) 
            : base(contentManager, effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.Aniso4xSampler);
        }
    }
}
