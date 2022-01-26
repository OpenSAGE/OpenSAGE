using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics.ParticleSystems
{
    [DebuggerDisplay("ParticleSystem {Template.Name}")]
    public sealed class ParticleSystem : DisposableBase, IPersistableObject
    {
        public const int KeyframeCount = 8;

        public delegate ref readonly Matrix4x4 GetMatrixReferenceDelegate();

        private readonly GetMatrixReferenceDelegate _getWorldMatrix;
        private readonly Matrix4x4 _worldMatrix;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly FXParticleEmissionVelocityBase _velocityType;
        private readonly FXParticleEmissionVolumeBase _volumeType;

        private readonly Material _particleMaterial;
        private readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ResourceSet _renderItemConstantsResourceSet;

        private readonly BeforeRenderDelegate _beforeRender;
        private bool _worldMatrixChanged;

        private int _initialDelay;

        private readonly float _startSizeRate;

        private float _startSize;

        internal readonly ParticleColorKeyframe[] ColorKeyframes = new ParticleColorKeyframe[KeyframeCount];

        private TimeSpan _nextUpdate;

        private int _timer;
        private int _nextBurst;

        private uint _systemId;
        private uint _attachedToDrawableId;
        private uint _attachedToObjectId;
        private bool _isIdentityTransform;
        private Matrix4x3 _transform;
        private bool _isIdentityTransform2;
        private Matrix4x3 _transform2;
        private uint _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private uint _unknownInt4;
        private uint _unknownInt5;
        private bool _hasInfiniteLifetime;
        private float _unknownFloat1;
        private bool _unknownBool1;
        private Vector3 _position;
        private Vector3 _positionPrevious;
        private bool _unknownBool2;
        private uint _slaveSystemId;
        private uint _masterSystemId;

        private Particle[] _particles;
        private readonly List<int> _deadList;

        private readonly DeviceBuffer _vertexBuffer;
        private readonly ParticleShaderResources.ParticleVertex[] _vertices;

        private readonly DeviceBuffer _indexBuffer;
        private readonly uint _numIndices;

        public FXParticleSystemTemplate Template { get; }

        public ParticleSystemState State { get; private set; }

        public int CurrentParticleCount { get; private set; }
        
        internal ParticleSystem(
            FXParticleSystemTemplate template,
            AssetLoadContext loadContext,
            GetMatrixReferenceDelegate getWorldMatrix)
            : this(template, loadContext)
        {
            _getWorldMatrix = getWorldMatrix;
        }

        internal ParticleSystem(
            FXParticleSystemTemplate template,
            AssetLoadContext loadContext,
            in Matrix4x4 worldMatrix)
            : this(template, loadContext)
        {
            _worldMatrix = worldMatrix;
        }

        private ParticleSystem(
            FXParticleSystemTemplate template,
            AssetLoadContext loadContext)
        {
            Template = template;

            var maxParticles = CalculateMaxParticles();

            // If this system never emits any particles, there's no reason to fully initialise it.
            if (maxParticles == 0)
            {
                return;
            }

            // TODO: This might not always be the right thing to do?
            if (template.ParticleTexture?.Value == null)
            {
                return;
            }

            _graphicsDevice = loadContext.GraphicsDevice;

            var particleShaderSet = loadContext.ShaderSetStore.GetParticleShaderSet();

            _particleMaterial = particleShaderSet.GetMaterial(Template);

            _renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(_graphicsDevice));

            _renderItemConstantsResourceSet = AddDisposable(
                _graphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        particleShaderSet.ResourceLayouts[3],
                        _renderItemConstantsBufferVS.Buffer)));

            _velocityType = Template.EmissionVelocity;
            _volumeType = Template.EmissionVolume;

            _initialDelay = Template.InitialDelay.GetRandomInt();

            _startSizeRate = Template.StartSizeRate.GetRandomFloat();
            _startSize = 0;

            for (var i = 0; i < KeyframeCount; i++)
            {
                ColorKeyframes[i] = new ParticleColorKeyframe(Template.Colors.ColorKeyframes[i]);
            }

            _particles = new Particle[maxParticles];
            for (var i = 0; i < _particles.Length; i++)
            {
                _particles[i] = new Particle(this);
            }

            _deadList = new List<int>();
            _deadList.AddRange(Enumerable.Range(0, maxParticles));

            var numVertices = maxParticles * 4;
            _vertexBuffer = AddDisposable(loadContext.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    (uint) (ParticleShaderResources.ParticleVertex.VertexDescriptor.Stride * numVertices),
                    BufferUsage.VertexBuffer | BufferUsage.Dynamic)));

            _vertices = new ParticleShaderResources.ParticleVertex[numVertices];

            _indexBuffer = AddDisposable(CreateIndexBuffer(
                loadContext.GraphicsDevice,
                maxParticles,
                out _numIndices));

            State = ParticleSystemState.Inactive;

            _beforeRender = (CommandList cl, RenderContext context, in RenderItem renderItem) =>
            {
                // Only update once we know this particle system is visible on screen.
                // We need to run enough updates to catch up for any time
                // the particle system has been offscreen.
                var anyUpdates = false;
                while (true)
                {
                    if (!Update(context.GameTime))
                    {
                        break;
                    }
                    anyUpdates = true;
                }

                if (anyUpdates)
                {
                    UpdateVertexBuffer(cl);
                }

                cl.SetVertexBuffer(0, _vertexBuffer);

                if (_worldMatrixChanged)
                {
                    _renderItemConstantsBufferVS.Update(cl);
                    _worldMatrixChanged = false;
                }

                cl.SetGraphicsResourceSet(3, _renderItemConstantsResourceSet);
            };
        }

        public void Activate()
        {
            if (State == ParticleSystemState.Inactive)
            {
                State = ParticleSystemState.Active;
            }
        }

        public void Deactivate()
        {
            if (State == ParticleSystemState.Active)
            {
                State = ParticleSystemState.Inactive;
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
            var maxLifetime = Template.SystemLifetime > 0
                ? Math.Min(Template.Lifetime.High, Template.SystemLifetime)
                : Template.Lifetime.High;
            return (int) Template.BurstCount.High + (int) MathF.Ceiling((maxLifetime / (Template.BurstDelay.Low + 1)) * Template.BurstCount.High);
        }

        private bool Update(in TimeInterval gameTime)
        {
            if (_particles == null)
            {
                return false;
            }

            if (gameTime.TotalTime < _nextUpdate)
            {
                return false;
            }

            if (_nextUpdate == TimeSpan.Zero)
            {
                _nextUpdate = gameTime.TotalTime;
            }

            _nextUpdate += TimeSpan.FromSeconds(1 / 30.0f);

            if (_initialDelay > 0)
            {
                _initialDelay -= 1;
                return false;
            }

            if (Template.SystemLifetime != 0 && _timer > Template.SystemLifetime)
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

            var particleCount = 0;

            for (var i = 0; i < _particles.Length; i++)
            {
                ref var particle = ref _particles[i];

                if (particle.Dead)
                {
                    continue;
                }

                UpdateParticle(ref particle);

                particleCount++;
            }

            CurrentParticleCount = particleCount;

            if (particleCount == 0 && State == ParticleSystemState.Finished)
            {
                State = ParticleSystemState.Dead;
            }

            _timer += 1;

            return true;
        }

        private void EmitParticles()
        {
            if (_nextBurst > 0)
            {
                _nextBurst -= 1;
                return;
            }

            _nextBurst = Template.BurstDelay.GetRandomInt();

            var burstCount = Template.BurstCount.GetRandomInt();

            for (var i = 0; i < burstCount; i++)
            {
                var ray = _volumeType.GetRay();

                var velocity = _velocityType?.GetVelocity(ray.Direction, Template.EmissionVolume) ?? Vector3.Zero;

                // TODO: Look at Definition.Type == Streak, etc.

                ref var newParticle = ref FindDeadParticleOrCreateNewOne();

                InitializeParticle(
                    ref newParticle,
                    ray.Position,
                    velocity,
                    _startSize);

                // TODO: Is this definitely incremented per particle, not per burst?
                _startSize = Math.Min(_startSize + _startSizeRate, 50);
            }
        }

        private void InitializeParticle(
            ref Particle particle, 
            in Vector3 position, 
            in Vector3 velocity, 
            float startSize)
        {
            particle.Dead = false;
            particle.Timer = 0;

            particle.Position = position;
            particle.Velocity = velocity;

            var update = (FXParticleUpdateDefault) Template.Update;

            particle.AngleZ = update.AngleZ.GetRandomFloat();
            particle.AngularRateZ = update.AngularRateZ.GetRandomFloat();
            particle.AngularDamping = update.AngularDamping.GetRandomFloat();

            particle.Lifetime = Template.Lifetime.GetRandomInt();

            particle.ColorScale = Template.Colors.ColorScale.GetRandomFloat();

            particle.Size = startSize + Template.Size.GetRandomFloat();
            particle.SizeRate = update.SizeRate.GetRandomFloat();
            particle.SizeRateDamping = update.SizeRateDamping.GetRandomFloat();

            var physics = (FXParticleDefaultPhysics) Template.Physics;
            particle.VelocityDamping = physics != null ? physics.VelocityDamping.GetRandomFloat() : 0.0f;

            for (var i = 0; i < KeyframeCount; i++)
            {
                particle.AlphaKeyframes[i] = new ParticleAlphaKeyframe(Template.Alpha.AlphaKeyframes[i]);
            }
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
            var physics = (FXParticleDefaultPhysics) Template.Physics;

            particle.Velocity *= particle.VelocityDamping;

            if (physics != null)
            {
                particle.Velocity.Z += physics.Gravity;
            }

            var totalVelocity = particle.Velocity;

            if (physics != null)
            {
                totalVelocity += physics.DriftVelocity;
            }

            particle.Position += totalVelocity;

            particle.Size = Math.Max(particle.Size + particle.SizeRate, 0.001f);
            particle.SizeRate *= particle.SizeRateDamping;

            particle.AngleZ += particle.AngularRateZ;
            particle.AngularRateZ *= particle.AngularDamping;

            FindKeyframes(particle.Timer, ColorKeyframes, out var nextC, out var prevC);

            if (!prevC.Equals(nextC))
            {
                var colorInterpoland = (float) (particle.Timer - prevC.Time) / (nextC.Time - prevC.Time);
                particle.Color = Vector3.Lerp(prevC.Color, nextC.Color, colorInterpoland);
            }
            else
            {
                particle.Color = prevC.Color;
            }
            var colorVal = particle.ColorScale * particle.Timer / 255.0f;
            particle.Color.X += colorVal;
            particle.Color.Y += colorVal;
            particle.Color.Z += colorVal;

            if (particle.AlphaKeyframes.Length > 1)
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

        private static void FindKeyframes<T>(
            int timer,
            T[] keyFrames,
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

        private void UpdateVertexBuffer(CommandList commandList)
        {
            var vertexIndex = 0;

            for (var i = 0; i < _particles.Length; i++)
            {
                ref var particle = ref _particles[i];

                var particleVertex = new ParticleShaderResources.ParticleVertex
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

            commandList.UpdateBuffer(_vertexBuffer, 0, _vertices);
        }

        internal void BuildRenderList(RenderList renderList)
        {
            if (_particles == null)
            {
                return;
            }

            ref readonly var worldMatrix = ref GetWorldMatrix();

            _worldMatrixChanged = false;
            if (worldMatrix != _renderItemConstantsBufferVS.Value.World)
            {
                _renderItemConstantsBufferVS.Value.World = worldMatrix;
                _worldMatrixChanged = true;
            }

            renderList.Transparent.RenderItems.Add(new RenderItem(
                Template.Name,
                _particleMaterial,
                AxisAlignedBoundingBox.CreateFromSphere(new BoundingSphere(worldMatrix.Translation, 10)), // TODO
                worldMatrix,
                0,
                _numIndices,
                _indexBuffer,
                _beforeRender));
        }

        private ref readonly Matrix4x4 GetWorldMatrix()
        {
            if (_getWorldMatrix != null)
            {
                return ref _getWorldMatrix();
            }
            else
            {
                return ref _worldMatrix;
            }
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObject(Template.LegacyTemplate, "TemplateData");

            reader.PersistUInt32(ref _systemId);
            reader.PersistUInt32(ref _attachedToDrawableId);
            reader.PersistObjectID(ref _attachedToObjectId);
            reader.PersistBoolean(ref _isIdentityTransform);
            reader.PersistMatrix4x3(ref _transform, readVersion: false);
            reader.PersistBoolean(ref _isIdentityTransform2);
            reader.PersistMatrix4x3(ref _transform2, readVersion: false);
            reader.PersistUInt32(ref _unknownInt1); // Maybe _nextBurst
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _unknownInt5);
            reader.PersistBoolean(ref _hasInfiniteLifetime);
            reader.PersistSingle(ref _unknownFloat1);
            reader.PersistBoolean(ref _unknownBool1);

            reader.BeginArray("UnknownFloats");
            for (var i = 0; i < 6; i++)
            {
                var unknown25 = 1.0f;
                reader.PersistSingleValue(ref unknown25);
                if (unknown25 != 1.0f)
                {
                    throw new InvalidStateException();
                }
            }
            reader.EndArray();

            reader.PersistVector3(ref _position);
            reader.PersistVector3(ref _positionPrevious);
            reader.PersistBoolean(ref _unknownBool2);
            reader.PersistUInt32(ref _slaveSystemId);
            reader.PersistUInt32(ref _masterSystemId);

            var numParticles = (uint)(_particles?.Length ?? 0);
            reader.PersistUInt32(ref numParticles);

            if (reader.Mode == StatePersistMode.Read)
            {
                // TODO: Shouldn't do this.
                _particles = new Particle[numParticles];
            }

            reader.BeginArray("Particles");
            for (var i = 0; i < numParticles; i++)
            {
                if (reader.Mode == StatePersistMode.Read)
                {
                    _particles[i] = new Particle(this);
                }
                reader.PersistObjectValue(ref _particles[i]);
            }
            reader.EndArray();
        }
    }

    public enum ParticleSystemState
    {
        Inactive,
        Active,
        Finished,
        Dead
    }

    internal struct ParticleColorKeyframe : IParticleKeyframe, IPersistableObject
    {
        public uint Time;
        public Vector3 Color;

        uint IParticleKeyframe.Time => Time;

        public ParticleColorKeyframe(RgbColorKeyframe keyframe)
        {
            Time = keyframe.Time;
            Color = keyframe.Color.ToVector3();
        }

        public ParticleColorKeyframe(uint time, in Vector3 color)
        {
            Time = time;
            Color = color;
        }

        public void Persist(StatePersister persister)
        {
            persister.PersistVector3(ref Color);
            persister.PersistUInt32(ref Time);
        }
    }

    internal interface IParticleKeyframe
    {
        uint Time { get; }
    }
}
