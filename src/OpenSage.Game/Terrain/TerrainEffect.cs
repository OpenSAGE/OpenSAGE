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
        private readonly DynamicBuffer<TransformConstants> _transformConstantBuffer;
        private TransformConstants _transformConstants;

        private readonly DynamicBuffer<LightingConstants> _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        private TerrainEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        private Texture _tileDataTexture;
        private StaticBuffer<CliffInfo> _cliffDetailsBuffer;
        private StaticBuffer<TextureInfo> _textureDetailsBuffer;
        private Texture _textureArray;

        [Flags]
        private enum TerrainEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x2,

            LightingConstants = 0x4,

            TileDataTexture = 0x8,
            CliffDetailsBuffer = 0x10,
            TextureDetailsBuffer = 0x20,
            Textures = 0x40,

            All = TransformConstants
                | LightingConstants
                | TileDataTexture
                | CliffDetailsBuffer
                | TextureDetailsBuffer
                | Textures
        }

        public TerrainEffect(GraphicsDevice graphicsDevice, int numTextures)
            : base(
                  graphicsDevice, 
                  "TerrainVS", 
                  "TerrainPS",
                  CreateVertexDescriptor())
        {
            _transformConstantBuffer = AddDisposable(DynamicBuffer<TransformConstants>.Create(graphicsDevice, BufferBindFlags.ConstantBuffer));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer<LightingConstants>.Create(graphicsDevice, BufferBindFlags.ConstantBuffer));
        }

        private static VertexDescriptor CreateVertexDescriptor()
        {
            return new VertexDescriptor(
                 new[]
                 {
                    new VertexAttributeDescription(InputClassification.PerVertexData, "POSITION", 0, VertexFormat.Float3, 0, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "NORMAL", 0, VertexFormat.Float3, 12, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 0, VertexFormat.Float2, 24, 0),

                 },
                 new[]
                 {
                    new VertexLayoutDescription(32)
                 });
        }

        protected override void OnBegin(CommandEncoder commandEncoder)
        {
            _dirtyFlags = TerrainEffectDirtyFlags.All;

            commandEncoder.SetFragmentSampler(0, GraphicsDevice.SamplerAnisotropicWrap);
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.World = _world;
                _transformConstants.WorldViewProjection = _world * _view * _projection;

                _transformConstantBuffer.UpdateData(ref _transformConstants);

                commandEncoder.SetVertexConstantBuffer(0, _transformConstantBuffer);

                _dirtyFlags &= ~TerrainEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.LightingConstants))
            {
                Matrix4x4.Invert(_view, out var viewInverse);
                _lightingConstants.CameraPosition = viewInverse.Translation;

                _lightingConstantBuffer.UpdateData(ref _lightingConstants);

                commandEncoder.SetFragmentConstantBuffer(0, _lightingConstantBuffer);

                _dirtyFlags &= ~TerrainEffectDirtyFlags.LightingConstants;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TileDataTexture))
            {
                commandEncoder.SetFragmentTexture(0, _tileDataTexture);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.TileDataTexture;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.CliffDetailsBuffer))
            {
                commandEncoder.SetFragmentStructuredBuffer(1, _cliffDetailsBuffer);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.CliffDetailsBuffer;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TextureDetailsBuffer))
            {
                commandEncoder.SetFragmentStructuredBuffer(2, _textureDetailsBuffer);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.TextureDetailsBuffer;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.Textures))
            {
                commandEncoder.SetFragmentTexture(3, _textureArray);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.Textures;
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
            _tileDataTexture = tileDataTexture;
            _dirtyFlags |= TerrainEffectDirtyFlags.TileDataTexture;
        }

        public void SetCliffDetails(StaticBuffer<CliffInfo> cliffDetailsBuffer)
        {
            _cliffDetailsBuffer = cliffDetailsBuffer;
            _dirtyFlags |= TerrainEffectDirtyFlags.CliffDetailsBuffer;
        }

        public void SetTextureDetails(StaticBuffer<TextureInfo> textureDetailsBuffer)
        {
            _textureDetailsBuffer = textureDetailsBuffer;
            _dirtyFlags |= TerrainEffectDirtyFlags.TextureDetailsBuffer;
        }

        public void SetTextureArray(Texture textureArray)
        {
            _textureArray = textureArray;
            _dirtyFlags |= TerrainEffectDirtyFlags.Textures;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }
    }
}
