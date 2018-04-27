using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class FixedFunctionMaterial : MeshMaterial
    {
        public FixedFunctionMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            
        }

        public void SetTexture0(Texture texture)
        {
            SetProperty("Texture0", texture);
        }

        public void SetTexture1(Texture texture)
        {
            SetProperty("Texture1", texture);
        }

        public void SetMaterialConstants(DeviceBuffer materialConstants)
        {
            SetProperty("MaterialConstants", materialConstants);
        }
    }
}
