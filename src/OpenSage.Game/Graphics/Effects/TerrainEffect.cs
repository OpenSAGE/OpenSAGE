using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public sealed class TerrainEffect : Effect
    {
        private readonly DynamicBuffer<TransformConstants> _transformConstantBuffer;
        private TransformConstants _transformConstants;

        private readonly DynamicBuffer<LightingConstants> _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        private TerrainEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        private ShaderResourceView _tileDataTexture;
        private ShaderResourceView _cliffDetailsBuffer;
        private ShaderResourceView _textureDetailsBuffer;
        private ShaderResourceView _textures;

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
                  CreateVertexDescriptor(),
                  CreatePipelineLayoutDescription(numTextures))
        {
            _transformConstantBuffer = AddDisposable(DynamicBuffer<TransformConstants>.Create(graphicsDevice));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer<LightingConstants>.Create(graphicsDevice));
        }

        private static VertexDescriptor CreateVertexDescriptor()
        {
            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            vertexDescriptor.SetAttributeDescriptor(2, "TEXCOORD", 0, VertexFormat.Float2, 0, 24);
            vertexDescriptor.SetLayoutDescriptor(0, 32);
            return vertexDescriptor;
        }

        private static PipelineLayoutDescription CreatePipelineLayoutDescription(int numTextures)
        {
            return new PipelineLayoutDescription
            {
                Entries = new[]
                {
                    // TransformCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Vertex,
                        ResourceType.ConstantBuffer,
                        0),

                    // LightingCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Pixel,
                        ResourceType.ConstantBuffer,
                        0),

                    // TileData
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.Texture,
                        0, 1),

                    // CliffDetails
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.StructuredBuffer,
                        1, 1),

                    // TextureDetails
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.StructuredBuffer,
                        2, 1),

                    // Textures
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.StructuredBuffer,
                        3, numTextures)
                },

                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription(
                        ShaderStageVisibility.Pixel,
                        0,
                        new SamplerStateDescription(SamplerFilter.Anisotropic))
                }
            };
        }

        protected override void OnBegin()
        {
            _dirtyFlags = TerrainEffectDirtyFlags.All;
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.World = _world;
                _transformConstants.WorldViewProjection = _world * _view * _projection;

                _transformConstantBuffer.UpdateData(ref _transformConstants);

                commandEncoder.SetInlineConstantBuffer(0, _transformConstantBuffer);

                _dirtyFlags &= ~TerrainEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.LightingConstants))
            {
                Matrix4x4.Invert(_view, out var viewInverse);
                _lightingConstants.CameraPosition = viewInverse.Translation;

                _lightingConstantBuffer.UpdateData(ref _lightingConstants);

                commandEncoder.SetInlineConstantBuffer(1, _lightingConstantBuffer);

                _dirtyFlags &= ~TerrainEffectDirtyFlags.LightingConstants;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TileDataTexture))
            {
                commandEncoder.SetShaderResourceView(2, _tileDataTexture);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.TileDataTexture;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.CliffDetailsBuffer))
            {
                commandEncoder.SetShaderResourceView(3, _cliffDetailsBuffer);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.CliffDetailsBuffer;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.TextureDetailsBuffer))
            {
                commandEncoder.SetShaderResourceView(4, _textureDetailsBuffer);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.TextureDetailsBuffer;
            }

            if (_dirtyFlags.HasFlag(TerrainEffectDirtyFlags.Textures))
            {
                commandEncoder.SetShaderResourceView(5, _textures);
                _dirtyFlags &= ~TerrainEffectDirtyFlags.Textures;
            }
        }

        public void SetWorld(ref Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
        }

        public void SetView(ref Matrix4x4 matrix)
        {
            _view = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
            _dirtyFlags |= TerrainEffectDirtyFlags.LightingConstants;
        }

        public void SetProjection(ref Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= TerrainEffectDirtyFlags.TransformConstants;
        }

        public void SetLights(ref Lights lights)
        {
            _lightingConstants.Lights = lights;
            _dirtyFlags |= TerrainEffectDirtyFlags.LightingConstants;
        }

        public void SetTileData(ShaderResourceView tileDataTexture)
        {
            _tileDataTexture = tileDataTexture;
            _dirtyFlags |= TerrainEffectDirtyFlags.TileDataTexture;
        }

        public void SetCliffDetails(ShaderResourceView cliffDetailsBuffer)
        {
            _cliffDetailsBuffer = cliffDetailsBuffer;
            _dirtyFlags |= TerrainEffectDirtyFlags.CliffDetailsBuffer;
        }

        public void SetTextureDetails(ShaderResourceView textureDetailsBuffer)
        {
            _textureDetailsBuffer = textureDetailsBuffer;
            _dirtyFlags |= TerrainEffectDirtyFlags.TextureDetailsBuffer;
        }

        public void SetTextures(ShaderResourceView textures)
        {
            _textures = textures;
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
