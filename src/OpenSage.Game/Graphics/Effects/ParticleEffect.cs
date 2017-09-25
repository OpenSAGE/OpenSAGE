using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;

namespace OpenSage.Graphics.Effects
{
    public sealed class ParticleEffect : Effect
    {
        private readonly DynamicBuffer<ParticleTransformConstants> _transformConstantBuffer;
        private ParticleTransformConstants _transformConstants;

        private ShaderResourceView _texture;

        private ParticleEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        [Flags]
        private enum ParticleEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x1,

            Texture = 0x2,

            All = TransformConstants | Texture
        }

        public ParticleEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "ParticleVS", 
                  "ParticlePS",
                  CreateVertexDescriptor(),
                  CreatePipelineLayoutDescription())
        {
            _transformConstantBuffer = DynamicBuffer<ParticleTransformConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer);
        }

        private static VertexDescriptor CreateVertexDescriptor()
        {
            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "TEXCOORD", 0, VertexFormat.Float, 0, 12);
            vertexDescriptor.SetAttributeDescriptor(2, "TEXCOORD", 1, VertexFormat.Float3, 0, 16);
            vertexDescriptor.SetAttributeDescriptor(3, "TEXCOORD", 2, VertexFormat.Float, 0, 28);
            vertexDescriptor.SetAttributeDescriptor(4, "TEXCOORD", 3, VertexFormat.Float, 0, 32);
            vertexDescriptor.SetLayoutDescriptor(0, 36);
            return vertexDescriptor;
        }

        private static PipelineLayoutDescription CreatePipelineLayoutDescription()
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

                    // ParticleTexture
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.Texture,
                        0, 1)
                },

                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription(
                        ShaderStageVisibility.Pixel,
                        0,
                        new SamplerStateDescription(SamplerFilter.MinMagMipLinear))
                }
            };
        }

        protected override void OnBegin()
        {
            _dirtyFlags = ParticleEffectDirtyFlags.All;
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(ParticleEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.WorldViewProjection = _world * _view * _projection;
                _transformConstants.World = _world;

                Matrix4x4.Invert(_view, out var viewInverse);
                _transformConstants.CameraPosition = viewInverse.Translation;

                _transformConstantBuffer.UpdateData(_transformConstants);

                commandEncoder.SetInlineConstantBuffer(0, _transformConstantBuffer);

                _dirtyFlags &= ~ParticleEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(ParticleEffectDirtyFlags.Texture))
            {
                commandEncoder.SetShaderResourceView(1, _texture);
                _dirtyFlags &= ~ParticleEffectDirtyFlags.Texture;
            }
        }

        public void SetWorld(ref Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= ParticleEffectDirtyFlags.TransformConstants;
        }

        public void SetView(Matrix4x4 matrix)
        {
            _view = matrix;
            _dirtyFlags |= ParticleEffectDirtyFlags.TransformConstants;
        }

        public void SetProjection(Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= ParticleEffectDirtyFlags.TransformConstants;
        }

        public void SetTexture(ShaderResourceView texture)
        {
            _texture = texture;
            _dirtyFlags |= ParticleEffectDirtyFlags.Texture;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ParticleTransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
            public Vector3 CameraPosition;
        }
    }
}
