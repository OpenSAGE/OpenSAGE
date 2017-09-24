using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.ParticleSystems.VelocityTypes;
using OpenSage.Graphics.ParticleSystems.VolumeTypes;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleSystem : GraphicsObject
    {
        private readonly IVelocityType _velocityType;
        private readonly IVolumeType _volumeType;

        private readonly ShaderResourceView _textureView;

        private readonly EffectPipelineStateHandle _pipelineStateHandle;

        private int _initialDelay;

        private float _startSizeRate;
        private float _startSize;

        private readonly List<ParticleSystemColorKeyframe> _colorKeyframes;

        private int _timer;
        private int _nextBurst;

        private readonly Particle[] _particles;
        private readonly List<int> _deadList;

        private readonly DynamicBuffer<ParticleVertex> _vertexBuffer;
        private readonly ParticleVertex[] _vertices;

        private readonly StaticBuffer<ushort> _indexBuffer;

        public ParticleSystemDefinition Definition { get; }

        public ParticleSystemState State { get; private set; }

        public ParticleSystem(
            ParticleSystemDefinition definition,
            ContentManager contentManager)
        {
            Definition = definition;

            _velocityType = VelocityTypeUtility.GetImplementation(definition.VelocityType);
            _volumeType = VolumeTypeUtility.GetImplementation(definition.VolumeType);

            var texturePath = Path.Combine("Art", "Textures", definition.ParticleName);
            var texture = contentManager.Load<Texture>(texturePath, uploadBatch: null);
            _textureView = AddDisposable(ShaderResourceView.Create(contentManager.GraphicsDevice, texture));

            _pipelineStateHandle = new EffectPipelineState(
                RasterizerStateDescription.CullBackSolid,
                DepthStencilStateDescription.DepthRead,
                BlendStateDescription.Opaque) // TODO: Use appropriate "Shader" from particle system.
                .GetHandle();

            _initialDelay = definition.InitialDelay.GetRandomInt();

            _startSizeRate = definition.StartSizeRate.GetRandomFloat();
            _startSize = 0;

            _colorKeyframes = new List<ParticleSystemColorKeyframe>();

            if (definition.Color1 != null)
            {
                _colorKeyframes.Add(new ParticleSystemColorKeyframe(definition.Color1));
            }

            void addColorKeyframe(RgbColorKeyframe keyframe, RgbColorKeyframe previous)
            {
                if (keyframe != null && keyframe.Time > previous.Time)
                {
                    _colorKeyframes.Add(new ParticleSystemColorKeyframe(keyframe));
                }
            }

            addColorKeyframe(definition.Color2, definition.Color1);
            addColorKeyframe(definition.Color3, definition.Color2);
            addColorKeyframe(definition.Color4, definition.Color3);
            addColorKeyframe(definition.Color5, definition.Color4);
            addColorKeyframe(definition.Color6, definition.Color5);
            addColorKeyframe(definition.Color7, definition.Color6);
            addColorKeyframe(definition.Color8, definition.Color7);

            var maxParticles = CalculateMaxParticles();

            _particles = new Particle[maxParticles];
            for (var i = 0; i < _particles.Length; i++)
            {
                _particles[i].Dead = true;
            }

            _deadList = new List<int>();
            _deadList.AddRange(Enumerable.Range(0, maxParticles));

            _vertexBuffer = AddDisposable(DynamicBuffer<ParticleVertex>.CreateArray(
                contentManager.GraphicsDevice,
                maxParticles * 4,
                BufferUsageFlags.None));

            _vertices = new ParticleVertex[_vertexBuffer.ElementCount];

            var uploadBatch = new ResourceUploadBatch(contentManager.GraphicsDevice);
            uploadBatch.Begin();

            var indices = new ushort[maxParticles * 2 * 3]; // Two triangles per particle.
            var indexCounter = 0;
            for (ushort i = 0; i < maxParticles * 4; i += 4)
            {
                indices[indexCounter++] = (ushort) (i + 0);
                indices[indexCounter++] = (ushort) (i + 1);
                indices[indexCounter++] = (ushort) (i + 2);

                indices[indexCounter++] = (ushort) (i + 1);
                indices[indexCounter++] = (ushort) (i + 2);
                indices[indexCounter++] = (ushort) (i + 3);
            }

            _indexBuffer = AddDisposable(StaticBuffer.Create(
                contentManager.GraphicsDevice,
                uploadBatch,
                indices));

            uploadBatch.End();

            State = ParticleSystemState.Active;
        }

        private int CalculateMaxParticles()
        {
            // TODO: Is this right?
            // How about IsOneShot?
            return (int) Math.Ceiling(((Definition.Lifetime.High + 1) / (Definition.BurstDelay.Low + 1)) * Definition.BurstCount.High);
        }

        public void Update()
        {
            if (_initialDelay > 0)
            {
                _initialDelay -= 1;
                return;
            }

            if (Definition.SystemLifetime != 0 && _timer > Definition.SystemLifetime)
            {
                State = ParticleSystemState.Finished;
            }

            if (State == ParticleSystemState.Active)
            {
                EmitParticles();
            }

            var anyAlive = false;

            for (var i = 0; i < _particles.Length; i++)
            {
                ref var particle = ref _particles[i];

                if (particle.Dead)
                {
                    continue;
                }

                UpdateParticle(ref particle);

                if (particle.Dead)
                {
                    _deadList.Add(i);
                }
                else
                {
                    anyAlive = true;
                }
            }

            UpdateVertexBuffer();

            if (!anyAlive && State == ParticleSystemState.Finished)
            {
                State = ParticleSystemState.Dead;
            }

            _timer += 1;
        }

        private void EmitParticles()
        {
            if (_nextBurst > 0)
            {
                _nextBurst -= 1;
                return;
            }

            _nextBurst = Definition.BurstDelay.GetRandomInt();

            var burstCount = Definition.BurstCount.GetRandomInt();

            for (var i = 0; i < burstCount; i++)
            {
                var ray = _volumeType.GetRay(Definition);

                var velocity = _velocityType.GetVelocity(Definition, ray.Direction);

                // TODO: Look at Definition.Type == Streak, etc.

                ref var newParticle = ref FindDeadParticleOrCreateNewOne();

                InitializeParticle(
                    ref newParticle,
                    ref ray.Position,
                    ref velocity,
                    _startSize);

                // TODO: Is this definitely incremented per particle, not per burst?
                _startSize = Math.Min(_startSize + _startSizeRate, 50);
            }
        }

        private void InitializeParticle(
            ref Particle particle, 
            ref Vector3 position, 
            ref Vector3 velocity, 
            float startSize)
        {
            particle.Dead = false;
            particle.Timer = 0;

            particle.Position = position;
            particle.Velocity = velocity;

            particle.AngleZ = Definition.AngleZ.GetRandomFloat();
            particle.AngularRateZ = Definition.AngularRateZ.GetRandomFloat();
            particle.AngularDamping = Definition.AngularDamping.GetRandomFloat();

            particle.Lifetime = Definition.Lifetime.GetRandomInt();

            particle.ColorScale = Definition.ColorScale.GetRandomFloat();

            particle.Size = startSize + Definition.Size.GetRandomFloat();
            particle.SizeRate = Definition.SizeRate.GetRandomFloat();
            particle.SizeRateDamping = Definition.SizeRateDamping.GetRandomFloat();

            particle.VelocityDamping = Definition.VelocityDamping.GetRandomFloat();
        }

        private ref Particle FindDeadParticleOrCreateNewOne()
        {
            if (_deadList.Count == 0)
            {
                throw new InvalidOperationException("Ran out of available particles; this should never happen.");
            }

            var first = _deadList[0];

            _deadList.RemoveAt(0);

            return ref _particles[first];
        }

        private void UpdateParticle(ref Particle particle)
        {
            particle.Timer += 1;

            if (particle.Timer > particle.Lifetime)
            {
                particle.Dead = true;
                return;
            }

            particle.Velocity.Z += Definition.Gravity;
            particle.Velocity *= particle.VelocityDamping;

            var totalVelocity = Definition.DriftVelocity.ToVector3() + particle.Velocity;
            particle.Position += totalVelocity;

            particle.Size = Math.Max(particle.Size + particle.SizeRate, 0.001f);
            particle.SizeRate *= particle.SizeRateDamping;

            particle.AngleZ += particle.AngularRateZ;
            particle.AngularRateZ *= particle.AngularDamping;

            // TODO: Interpolate color and alpha values.
        }

        private void UpdateVertexBuffer()
        {
            var vertexIndex = 0;

            for (var i = 0; i < _particles.Length; i++)
            {
                ref var particle = ref _particles[i];

                var particleVertex = new ParticleVertex
                {
                    Position = particle.Position,
                    Size = particle.Dead ? 0 : particle.Size
                };

                // Repeat vertices 4 times; in the vertex shader, these will be transformed
                // into the 4 corners of a quad.
                _vertices[vertexIndex++] = particleVertex;
                _vertices[vertexIndex++] = particleVertex;
                _vertices[vertexIndex++] = particleVertex;
                _vertices[vertexIndex++] = particleVertex;
            }

            _vertexBuffer.UpdateData(_vertices);
        }

        public void Draw(
            CommandEncoder commandEncoder,
            ParticleEffect effect,
            Camera camera,
            ref Matrix4x4 world)
        {
            effect.SetPipelineState(_pipelineStateHandle);

            effect.SetWorld(ref world);
            effect.SetView(camera.ViewMatrix);
            effect.SetProjection(camera.ProjectionMatrix);

            effect.SetTexture(_textureView);

            effect.Apply(commandEncoder);

            commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            commandEncoder.DrawIndexed(
                PrimitiveType.TriangleList,
                _indexBuffer.ElementCount,
                _indexBuffer,
                0);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ParticleVertex
        {
            public Vector3 Position;
            public float Size;
        }
    }

    public enum ParticleSystemState
    {
        Active,
        Finished,
        Dead
    }

    internal sealed class ParticleSystemColorKeyframe
    {
        public int Time;
        public Vector3 Color;

        public ParticleSystemColorKeyframe(RgbColorKeyframe keyframe)
        {
            Time = keyframe.Time;
            Color = keyframe.Color.ToVector3();
        }
    }
}
