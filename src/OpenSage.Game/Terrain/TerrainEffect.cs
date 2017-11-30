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
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        [Flags]
        private enum TerrainEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x1,

            All = TransformConstants
        }

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
                SetConstantBufferField("TransformCB", "WorldViewProjection", _world * _view * _projection);
                SetConstantBufferField("TransformCB", "World", ref _world);

                Matrix4x4.Invert(_view, out var viewInverse);
                SetConstantBufferField("LightingCB", "CameraPosition", viewInverse.Translation);

                _dirtyFlags &= ~TerrainEffectDirtyFlags.TransformConstants;
            }
        }

        public void SetWorld(Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
        }

        public void SetView(Matrix4x4 matrix)
        {
            _view = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
        }

        public void SetProjection(Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
        }

        public void SetLights(ref Lights lights)
        {
            SetConstantBufferField("LightingCB", "Lights", ref lights);
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
