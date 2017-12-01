using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Terrain
{
    public sealed class TerrainEffect : Effect
    {
        public TerrainEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "TerrainVS", 
                  "TerrainPS",
                  TerrainVertex.VertexDescriptor)
        {
        }
    }

    public sealed class TerrainMaterial : EffectMaterial
    {
        public TerrainMaterial(TerrainEffect effect)
            : base(effect)
        {
            SetProperty("Sampler", effect.GraphicsDevice.SamplerAnisotropicWrap);
        }

        public void SetTileData(Texture tileDataTexture)
        {
            SetProperty("TileData", tileDataTexture);
        }

        public void SetCliffDetails(Buffer<CliffInfo> cliffDetailsBuffer)
        {
            SetProperty("CliffDetails", cliffDetailsBuffer);
        }

        public void SetTextureDetails(Buffer<TextureInfo> textureDetailsBuffer)
        {
            SetProperty("TextureDetails", textureDetailsBuffer);
        }

        public void SetTextureArray(Texture textureArray)
        {
            SetProperty("Textures", textureArray);
        }
    }
}
