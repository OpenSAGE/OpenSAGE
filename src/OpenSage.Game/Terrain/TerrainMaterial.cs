using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class TerrainMaterial : EffectMaterial
    {
        public TerrainMaterial(ContentManager contentManager, Effect effect)
            : base(contentManager, effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.Aniso4xSampler);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                BlendStateDescription.SingleDisabled);
        }

        public void SetTileData(Texture tileDataTexture)
        {
            SetProperty("TileData", tileDataTexture);
        }

        public void SetCliffDetails(DeviceBuffer cliffDetailsBuffer)
        {
            SetProperty("CliffDetails", cliffDetailsBuffer);
        }

        public void SetTextureDetails(DeviceBuffer textureDetailsBuffer)
        {
            SetProperty("TextureDetails", textureDetailsBuffer);
        }

        public void SetTextureArray(Texture textureArray)
        {
            SetProperty("Textures", textureArray);
        }
    }
}
