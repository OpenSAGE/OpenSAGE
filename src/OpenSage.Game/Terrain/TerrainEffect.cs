using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using LLGfx.Effects;
using OpenSage.Graphics.Effects;

namespace OpenSage.Terrain
{
    public sealed class TerrainEffect : Effect, IEffectMatrices, IEffectLights
    {
        private readonly Buffer<TransformConstants> _transformConstantBuffer;
        private TransformConstants _transformConstants;

        private readonly Buffer<LightingConstants> _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        private TerrainEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        [Flags]
        private enum TerrainEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x2,

            LightingConstants = 0x4,

            All = TransformConstants
                | LightingConstants
        }

        public TerrainEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "TerrainVS", 
                  "TerrainPS",
                  TerrainVertex.VertexDescriptor)
        {
            _transformConstantBuffer = AddDisposable(Buffer<TransformConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer));

            _lightingConstantBuffer = AddDisposable(Buffer<LightingConstants>.CreateDynamic(graphicsDevice, BufferBindFlags.ConstantBuffer));
        }

        protected override void OnBegin(CommandEncoder commandEncoder)
        {
            _dirtyFlags = TerrainEffectDirtyFlags.All;

            SetValue("Sampler", GraphicsDevice.SamplerAnisotropicWrap);
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.World = _world;
                _transformConstants.WorldViewProjection = _world * _view * _projection;

                _transformConstantBuffer.SetData(ref _transformConstants);

                commandEncoder.SetVertexShaderConstantBuffer(0, _transformConstantBuffer);

                _dirtyFlags &= ~TerrainEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.LightingConstants))
            {
                Matrix4x4.Invert(_view, out var viewInverse);
                _lightingConstants.CameraPosition = viewInverse.Translation;

                _lightingConstantBuffer.SetData(ref _lightingConstants);

                commandEncoder.SetPixelShaderConstantBuffer(0, _lightingConstantBuffer);

                _dirtyFlags &= ~TerrainEffectDirtyFlags.LightingConstants;
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
            _dirtyFlags |= TerrainEffectDirtyFlags.LightingConstants;
        }

        public void SetProjection(Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
        }

        public void SetLights(ref Lights lights)
        {
            _lightingConstants.Lights = lights;
            _dirtyFlags |= TerrainEffectDirtyFlags.LightingConstants;
        }

        public void SetTileData(Texture tileDataTexture)
        {
            SetValue("TileData", tileDataTexture);
        }

        public void SetCliffDetails(Buffer<CliffInfo> cliffDetailsBuffer)
        {
            SetValue("CliffDetails", cliffDetailsBuffer);
        }

        public void SetTextureDetails(Buffer<TextureInfo> textureDetailsBuffer)
        {
            SetValue("TextureDetails", textureDetailsBuffer);
        }

        public void SetTextureArray(Texture textureArray)
        {
            SetValue("Textures", textureArray);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }
    }
}
