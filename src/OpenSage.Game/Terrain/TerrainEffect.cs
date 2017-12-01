using System;
using System.Numerics;
using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Terrain
{
    public sealed class TerrainEffect : Effect, IEffectMatrices, IEffectLights
    {
        private TerrainEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;

        [Flags]
        private enum TerrainEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x1,

            All = TransformConstants
        }

        LightingType IEffectLights.LightingType => LightingType.Object;

        public TerrainEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "TerrainVS", 
                  "TerrainPS",
                  TerrainVertex.VertexDescriptor)
        {
        }

        protected override void OnBegin()
        {
            _dirtyFlags = TerrainEffectDirtyFlags.All;
        }

        protected override void OnApply()
        {
            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TransformConstants))
            {
                SetConstantBufferField("TransformConstants", "World", ref _world);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.TransformConstants;
            }
        }

        void IEffectMatrices.SetWorld(Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
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
