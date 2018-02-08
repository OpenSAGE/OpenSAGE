using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.ParticleSystems.VelocityTypes;
using OpenSage.Graphics.ParticleSystems.VolumeTypes;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics.ParticleSystems
{

    public sealed class ParticleSystem : DisposableBase
    {
        public delegate ref readonly Matrix4x4 GetMatrixReferenceDelegate();

        private readonly GetMatrixReferenceDelegate _getWorldMatrix;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly IVelocityType _velocityType;
        private readonly IVolumeType _volumeType;

        private readonly ParticleMaterial _particleMaterial;

        private int _initialDelay;

        private readonly float _startSizeRate;

        private float _startSize;

        private readonly List<ParticleColorKeyframe> _colorKeyframes;

        private TimeSpan _nextUpdate;

        private int _timer;
        private int _nextBurst;

        private readonly Particle[] _particles;
        private readonly List<int> _deadList;

        private readonly DeviceBuffer _vertexBuffer;
        private readonly ParticleVertex[] _vertices;

        private readonly DeviceBuffer _indexBuffer;
        private readonly uint _numIndices;

        public ParticleSystemDefinition Definition { get; }

        public ParticleSystemState State { get; private set; }

        public ParticleSystem(
            ContentManager contentManager,
            ParticleSystemDefinition definition,
            GetMatrixReferenceDelegate getWorldMatrix)
        {
            Definition = definition;

            _getWorldMatrix = getWorldMatrix;

            _graphicsDevice = contentManager.GraphicsDevice;

            _particleMaterial = AddDisposable(new ParticleMaterial(contentManager, contentManager.EffectLibrary.Particle));

            _velocityType = VelocityTypeUtility.GetImplementation(Definition.VelocityType);
            _volumeType = VolumeTypeUtility.GetImplementation(Definition.VolumeType);

            var texturePath = Path.Combine("Art", "Textures", Definition.ParticleName);
            var texture = contentManager.Load<Texture>(texturePath);
            _particleMaterial.SetTexture(texture);

            var blendState = GetBlendState(Definition.Shader);

            _particleMaterial.PipelineState = new EffectPipelineState(
                RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                DepthStencilStateDescription.DepthOnlyLessEqualRead,
                blendState,
                contentManager.GraphicsDevice.SwapchainFramebuffer.OutputDescription);

            _initialDelay = Definition.InitialDelay.GetRandomInt();

            _startSizeRate = Definition.StartSizeRate.GetRandomFloat();
            _startSize = 0;

            _colorKeyframes = new List<ParticleColorKeyframe>();

            if (Definition.Color1 != null)
            {
                _colorKeyframes.Add(new ParticleColorKeyframe(Definition.Color1));
            }

            void addColorKeyframe(RgbColorKeyframe keyframe, RgbColorKeyframe previous)
            {
                if (keyframe != null && keyframe.Time > previous.Time)
                {
                    _colorKeyframes.Add(new ParticleColorKeyframe(keyframe));
                }
            }

            addColorKeyframe(Definition.Color2, Definition.Color1);
            addColorKeyframe(Definition.Color3, Definition.Color2);
            addColorKeyframe(Definition.Color4, Definition.Color3);
            addColorKeyframe(Definition.Color5, Definition.Color4);
            addColorKeyframe(Definition.Color6, Definition.Color5);
            addColorKeyframe(Definition.Color7, Definition.Color6);
            addColorKeyframe(Definition.Color8, Definition.Color7);

            var maxParticles = CalculateMaxParticles();

            _particles = new Particle[maxParticles];
            for (var i = 0; i < _particles.Length; i++)
            {
                _particles[i].AlphaKeyframes = new List<ParticleAlphaKeyframe>();
                _particles[i].Dead = true;
            }

            _deadList = new List<int>();
            _deadList.AddRange(Enumerable.Range(0, maxParticles));

            var numVertices = maxParticles * 4;
            _vertexBuffer = AddDisposable(contentManager.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    (uint) (ParticleVertex.VertexDescriptor.Stride * maxParticles * 4),
                    BufferUsage.VertexBuffer | BufferUsage.Dynamic)));

            _vertices = new ParticleVertex[numVertices];

            _indexBuffer = AddDisposable(CreateIndexBuffer(
                contentManager.GraphicsDevice,
                maxParticles,
                out _numIndices));

            State = ParticleSystemState.Active;
        }

        private static BlendStateDescription GetBlendState(ParticleSystemShader shader)
        {
            switch (shader)
            {
                case ParticleSystemShader.Alpha:
                    return BlendStateDescription.SingleAlphaBlend;

                case ParticleSystemShader.Additive:
                    return BlendStateDescription.SingleAdditiveBlend;

                default:
                    throw new ArgumentOutOfRangeException(nameof(shader));
            }
        }

        private static DeviceBuffer CreateIndexBuffer(GraphicsDevice graphicsDevice, int maxParticles, out uint numIndices)
        {
            numIndices = (uint) maxParticles * 2 * 3; // Two triangles per particle.
            var indices = new ushort[numIndices]; 
            var indexCounter = 0;
            for (ushort i = 0; i < maxParticles * 4; i += 4)
            {
                indices[indexCounter++] = (ushort) (i + 0);
                indices[indexCounter++] = (ushort) (i + 2);
                indices[indexCounter++] = (ushort) (i + 1);

                indices[indexCounter++] = (ushort) (i + 1);
                indices[indexCounter++] = (ushort) (i + 2);
                indices[indexCounter++] = (ushort) (i + 3);
            }

            var result = graphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer);

            return result;
        }

        private int CalculateMaxParticles()
        {
            // TODO: Is this right?
            // How about IsOneShot?
            return (int) Definition.BurstCount.High + (int) Math.Ceiling(((Definition.Lifetime.High) / (Definition.BurstDelay.Low + 1)) * Definition.BurstCount.High);
        }

        public void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime < _nextUpdate)
            {
                return;
            }

            _nextUpdate = gameTime.TotalGameTime + TimeSpan.FromSeconds(1 / 30.0f);

            if (_initialDelay > 0)
            {
                _initialDelay -= 1;
                return;
            }

            if (Definition.SystemLifetime != 0 && _timer > Definition.SystemLifetime)
            {
                State = ParticleSystemState.Finished;
            }

            for (var i = 0; i < _particles.Length; i++)
            {
                ref var particle = ref _particles[i];

                if (particle.Dead)
                {
                    continue;
                }

                if (particle.Timer > particle.Lifetime)
                {
                    particle.Dead = true;
                    _deadList.Add(i);
                }
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

                anyAlive = true;
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

            var alphaKeyframes = particle.AlphaKeyframes;
            alphaKeyframes.Clear();

            if (Definition.Alpha1 != null)
            {
                alphaKeyframes.Add(new ParticleAlphaKeyframe(Definition.Alpha1));
            }

            void addAlphaKeyframe(RandomAlphaKeyframe keyframe, RandomAlphaKeyframe previous)
            {
                if (keyframe != null && keyframe.Time > previous.Time)
                {
                    alphaKeyframes.Add(new ParticleAlphaKeyframe(keyframe));
                }
            }

            addAlphaKeyframe(Definition.Alpha2, Definition.Alpha1);
            addAlphaKeyframe(Definition.Alpha3, Definition.Alpha2);
            addAlphaKeyframe(Definition.Alpha4, Definition.Alpha3);
            addAlphaKeyframe(Definition.Alpha5, Definition.Alpha4);
            addAlphaKeyframe(Definition.Alpha6, Definition.Alpha5);
            addAlphaKeyframe(Definition.Alpha7, Definition.Alpha6);
            addAlphaKeyframe(Definition.Alpha8, Definition.Alpha7);
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
            particle.Velocity.Z += Definition.Gravity;
            particle.Velocity *= particle.VelocityDamping;

            var totalVelocity = Definition.DriftVelocity.ToVector3() + particle.Velocity;
            particle.Position += totalVelocity;

            particle.Size = Math.Max(particle.Size + particle.SizeRate, 0.001f);
            particle.SizeRate *= particle.SizeRateDamping;

            particle.AngleZ += particle.AngularRateZ;
            particle.AngularRateZ *= particle.AngularDamping;

            FindKeyframes(particle.Timer, _colorKeyframes, out var nextC, out var prevC);

            if (!prevC.Equals(nextC))
            {
                var colorInterpoland = (float) (particle.Timer - prevC.Time) / (nextC.Time - prevC.Time);
                particle.Color = Vector3Utility.Lerp(in prevC.Color, in nextC.Color, colorInterpoland);
            }
            else
            {
                particle.Color = prevC.Color;
            }
            var colorVal = particle.ColorScale * particle.Timer / 255.0f;
            particle.Color.X += colorVal;
            particle.Color.Y += colorVal;
            particle.Color.Z += colorVal;

            if (particle.AlphaKeyframes.Count > 1)
            {
                FindKeyframes(particle.Timer, particle.AlphaKeyframes, out var nextA, out var prevA);

                if (!prevA.Equals(nextA))
                {
                    var alphaInterpoland = (float) (particle.Timer - prevA.Time) / (nextA.Time - prevA.Time);
                    particle.Alpha = MathUtility.Lerp(prevA.Alpha, nextA.Alpha, alphaInterpoland);
                }
                else
                {
                    particle.Alpha = prevA.Alpha;
                }
            }
            else
            {
                particle.Alpha = 1;
            }

            particle.Timer += 1;
        }

        private static void FindKeyframes<T>(int timer,
            IReadOnlyList<T> keyFrames,
            out T next, out T prev)
            where T : struct, IParticleKeyframe
        {
            prev = keyFrames[0];
            next = prev;

            foreach (var keyFrame in keyFrames)
            {
                if (keyFrame.Time >= timer)
                {
                    next = keyFrame;
                    break;
                }

                prev = keyFrame;
            }
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
                    Size = particle.Dead ? 0 : particle.Size,
                    Color = particle.Color,
                    Alpha = particle.Alpha,
                    AngleZ = particle.AngleZ,
                };

                // Repeat vertices 4 times; in the vertex shader, these will be transformed
                // into the 4 corners of a quad.
                _vertices[vertexIndex++] = particleVertex;
                _vertices[vertexIndex++] = particleVertex;
                _vertices[vertexIndex++] = particleVertex;
                _vertices[vertexIndex++] = particleVertex;
            }

            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices);
        }

        public ref readonly Matrix4x4 GetWorldMatrix() => ref _getWorldMatrix();

        public void BuildRenderList(RenderList renderList, in Matrix4x4 worldMatrix)
        {
            renderList.Transparent.AddRenderItemDrawIndexed(
                _particleMaterial,
                _vertexBuffer,
                null,
                CullFlags.None,
                BoundingBox.CreateFromSphere(new BoundingSphere(worldMatrix.Translation, 100)), // TODO
                worldMatrix,
                0,
                _numIndices,
                _indexBuffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ParticleVertex
    {
        public Vector3 Position;
        public float Size;
        public Vector3 Color;
        public float Alpha;
        public float AngleZ;

        public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
            new VertexElementDescription("POSITION", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1));
    }

    public enum ParticleSystemState
    {
        Active,
        Finished,
        Dead
    }

    internal readonly struct ParticleColorKeyframe : IParticleKeyframe
    {
        public int Time { get; }
        public readonly Vector3 Color;

        public ParticleColorKeyframe(RgbColorKeyframe keyframe)
        {
            Time = keyframe.Time;
            Color = keyframe.Color.ToVector3();
        }
    }

    internal interface IParticleKeyframe
    {
        int Time { get; }
    }
}
