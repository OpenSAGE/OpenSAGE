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

        private ParticleEffectDirtyFlags _dirtyFlags;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        [Flags]
        private enum ParticleEffectDirtyFlags
        {
            None = 0,

            TransformConstants = 0x1,

            All = TransformConstants
        }

        public ParticleEffect(GraphicsDevice graphicsDevice) 
            : base(
                  graphicsDevice, 
                  "ParticleVS", 
                  "ParticlePS",
                  ParticleVertex.VertexDescriptor)
        {
            _transformConstantBuffer = DynamicBuffer<ParticleTransformConstants>.Create(graphicsDevice, BufferBindFlags.ConstantBuffer);
        }

        protected override void OnBegin(CommandEncoder commandEncoder)
        {
            _dirtyFlags = ParticleEffectDirtyFlags.All;

            SetValue("LinearSampler", GraphicsDevice.SamplerLinearWrap);
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
            SetValue("ParticleTexture", texture);
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
