using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using LLGfx.Effects;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleEffect : Effect, IEffectMatrices
    {
        private readonly DynamicBuffer<ParticleTransformConstants> _transformConstantBuffer;
        private ParticleTransformConstants _transformConstants;

        private Texture _texture;

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
                  CreateVertexDescriptor())
        {
            _transformConstantBuffer = DynamicBuffer<ParticleTransformConstants>.Create(graphicsDevice, BufferBindFlags.ConstantBuffer);
        }

        private static VertexDescriptor CreateVertexDescriptor()
        {
            return new VertexDescriptor(
                 new[]
                 {
                    new VertexAttributeDescription(InputClassification.PerVertexData, "POSITION", 0, VertexFormat.Float3, 0, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 0, VertexFormat.Float, 12, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 1, VertexFormat.Float3, 16, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 2, VertexFormat.Float, 28, 0),
                    new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 3, VertexFormat.Float, 32, 0),

                 },
                 new[]
                 {
                    new VertexLayoutDescription(36)
                 });
        }

        protected override void OnBegin(CommandEncoder commandEncoder)
        {
            _dirtyFlags = ParticleEffectDirtyFlags.All;

            commandEncoder.SetFragmentSampler(0, GraphicsDevice.SamplerLinearWrap);
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(ParticleEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.World = _world;
                _transformConstants.ViewProjection = _view * _projection;

                var result = Matrix4x4.Invert(_view, out var viewInverse);
                _transformConstants.CameraPosition = viewInverse.Translation;

                _transformConstantBuffer.UpdateData(_transformConstants);

                commandEncoder.SetVertexConstantBuffer(0, _transformConstantBuffer);

                _dirtyFlags &= ~ParticleEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(ParticleEffectDirtyFlags.Texture))
            {
                commandEncoder.SetFragmentTexture(0, _texture);
                _dirtyFlags &= ~ParticleEffectDirtyFlags.Texture;
            }
        }

        public void SetWorld(Matrix4x4 matrix)
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

        public void SetTexture(Texture texture)
        {
            _texture = texture;
            _dirtyFlags |= ParticleEffectDirtyFlags.Texture;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ParticleTransformConstants
        {
            public Matrix4x4 World;
            public Matrix4x4 ViewProjection;
            public Vector3 CameraPosition;
        }
    }
}
