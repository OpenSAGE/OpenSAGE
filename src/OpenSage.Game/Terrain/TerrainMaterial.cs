using System.Numerics;
using System.Runtime.InteropServices;
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
            SetProperty("Sampler", contentManager.GraphicsDevice.Aniso4xSampler);

            PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                BlendStateDescription.SingleDisabled,
                contentManager.GraphicsDevice.SwapchainFramebuffer.OutputDescription);
        }

        public override LightingType LightingType => LightingType.Terrain;

        public void SetMaterialConstants(DeviceBuffer materialConstantsBuffer)
        {
            SetProperty("TerrainMaterialConstantsBuffer", materialConstantsBuffer);
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

        public void SetMacroTexture(Texture macroTexture)
        {
            SetProperty("MacroTexture", macroTexture);
        }

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        public struct TerrainMaterialConstants
        {
            [FieldOffset(0)]
            public Vector2 MapBorderWidth;

            [FieldOffset(8)]
            public Vector2 MapSize;

            [FieldOffset(16)]
            public bool IsMacroTextureStretched;
        }
    }
}
