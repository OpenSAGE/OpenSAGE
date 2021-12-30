using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics.ParticleSystems
{
    [DebuggerDisplay("ParticleSystem {Template.Name}")]
    public sealed class ParticleSystem : DisposableBase
    {
        public delegate ref readonly Matrix4x4 GetMatrixReferenceDelegate();

        private readonly GetMatrixReferenceDelegate _getWorldMatrix;
        private readonly Matrix4x4 _worldMatrix;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly FXParticleEmissionVelocityBase _velocityType;
        private readonly FXParticleEmissionVolumeBase _volumeType;

        private readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ConstantBuffer<ParticleShaderResources.ParticleConstantsVS> _particleConstantsBufferVS;
        private readonly ResourceSet _particleResourceSet;
        private readonly ShaderSet _shaderSet;
        private readonly Pipeline _pipeline;

        private readonly BeforeRenderDelegate _beforeRender;
        private bool _worldMatrixChanged;

        private int _initialDelay;

        private readonly float _startSizeRate;

        private float _startSize;

        private readonly List<ParticleColorKeyframe> _colorKeyframes;

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

            _renderItemConstantsBufferVS = AddDisposable(new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(_graphicsDevice));

            _particleConstantsBufferVS = AddDisposable(new ConstantBuffer<ParticleShaderResources.ParticleConstantsVS>(_graphicsDevice));
            _particleConstantsBufferVS.Value.IsGroundAligned = template.IsGroundAligned;
            _particleConstantsBufferVS.Update(loadContext.GraphicsDevice);

            _velocityType = Template.EmissionVelocity;
            _volumeType = Template.EmissionVolume;

            _particleResourceSet = AddDisposable(loadContext.ShaderResources.Particle.CreateParticleResoureSet(
                _renderItemConstantsBufferVS.Buffer,
                _particleConstantsBufferVS.Buffer,
                Template.ParticleTexture.Value));

            _shaderSet = loadContext.ShaderResources.Particle.ShaderSet;
            _pipeline = loadContext.ShaderResources.Particle.GetCachedPipeline(Template.Shader);

            _initialDelay = Template.InitialDelay.GetRandomInt();

            _startSizeRate = Template.StartSizeRate.GetRandomFloat();
            _startSize = 0;

            _colorKeyframes = new List<ParticleColorKeyframe>();

            var colors = Template.Colors;

            if (colors.Color1 != null)
            {
                _colorKeyframes.Add(new ParticleColorKeyframe(colors.Color1));
            }

            void addColorKeyframe(RgbColorKeyframe keyframe, RgbColorKeyframe previous)
            {
                if (keyframe != null && keyframe.Time > previous.Time)
                {
                    _colorKeyframes.Add(new ParticleColorKeyframe(keyframe));
                }
            }

            addColorKeyframe(colors.Color2, colors.Color1);
            addColorKeyframe(colors.Color3, colors.Color2);
            addColorKeyframe(colors.Color4, colors.Color3);
            addColorKeyframe(colors.Color5, colors.Color4);
            addColorKeyframe(colors.Color6, colors.Color5);
            addColorKeyframe(colors.Color7, colors.Color6);
            addColorKeyframe(colors.Color8, colors.Color7);

            _particles = new Particle[maxParticles];
            for (var i = 0; i < _particles.Length; i++)
            {
                _particles[i].AlphaKeyframes = new List<ParticleAlphaKeyframe>();
                _particles[i].ColorKeyframes = new List<ParticleColorKeyframe>();
                _particles[i].Dead = true;
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

            _beforeRender = (cl, context) =>
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

                if (_worldMatrixChanged)
                {
                    _renderItemConstantsBufferVS.Update(cl);
                }

                cl.SetGraphicsResourceSet(1, _particleResourceSet);

                cl.SetVertexBuffer(0, _vertexBuffer);
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

            var alphaKeyframes = particle.AlphaKeyframes;
            alphaKeyframes.Clear();

            var alphas = Template.Alpha;

            if (alphas != null)
            {
                if (alphas.Alpha1 != null)
                {
                    alphaKeyframes.Add(new ParticleAlphaKeyframe(alphas.Alpha1));
                }

                void addAlphaKeyframe(RandomAlphaKeyframe keyframe, RandomAlphaKeyframe previous)
                {
                    if (keyframe != null && previous != null && keyframe.Time > previous.Time)
                    {
                        alphaKeyframes.Add(new ParticleAlphaKeyframe(keyframe));
                    }
                }

                addAlphaKeyframe(alphas.Alpha2, alphas.Alpha1);
                addAlphaKeyframe(alphas.Alpha3, alphas.Alpha2);
                addAlphaKeyframe(alphas.Alpha4, alphas.Alpha3);
                addAlphaKeyframe(alphas.Alpha5, alphas.Alpha4);
                addAlphaKeyframe(alphas.Alpha6, alphas.Alpha5);
                addAlphaKeyframe(alphas.Alpha7, alphas.Alpha6);
                addAlphaKeyframe(alphas.Alpha8, alphas.Alpha7);
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

            FindKeyframes(particle.Timer, _colorKeyframes, out var nextC, out var prevC);

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
                _shaderSet,
                _pipeline,
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

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            LoadTemplateData(reader);

            reader.PersistUInt32(ref _systemId);
            reader.PersistUInt32(ref _attachedToDrawableId);
            reader.PersistObjectID(ref _attachedToObjectId);
            reader.PersistBoolean(ref _isIdentityTransform);
            reader.PersistMatrix4x3Transposed(ref _transform);
            reader.PersistBoolean(ref _isIdentityTransform2);
            reader.PersistMatrix4x3Transposed(ref _transform2);
            reader.PersistUInt32(ref _unknownInt1); // Maybe _nextBurst
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _unknownInt5);
            reader.PersistBoolean(ref _hasInfiniteLifetime);
            reader.PersistSingle(ref _unknownFloat1);
            reader.PersistBoolean(ref _unknownBool1);

            for (var i = 0; i < 6; i++)
            {
                var unknown25 = 1.0f;
                reader.PersistSingle(ref unknown25);
                if (unknown25 != 1.0f)
                {
                    throw new InvalidStateException();
                }
            }

            reader.PersistVector3(ref _position);
            reader.PersistVector3(ref _positionPrevious);
            reader.PersistBoolean(ref _unknownBool2);
            reader.PersistUInt32(ref _slaveSystemId);
            reader.PersistUInt32(ref _masterSystemId);

            var numParticles = 0u;
            reader.PersistUInt32(ref numParticles);

            // TODO: Shouldn't do this.
            _particles = new Particle[numParticles];

            for (var i = 0; i < numParticles; i++)
            {
                _particles[i] = new Particle();
                _particles[i].AlphaKeyframes = new List<ParticleAlphaKeyframe>();
                _particles[i].ColorKeyframes = new List<ParticleColorKeyframe>();
                _particles[i].Load(reader);
            }
        }

        private void LoadTemplateData(StatePersister reader)
        {
            // What follows is almost an exact replica of ParticleSystemTemplate,
            // with a few extra fields here and there.

            reader.PersistVersion(1);

            var isOneShot = false;
            reader.PersistBoolean(ref isOneShot); // IsOneShot

            ParticleSystemShader shader = default;
            reader.PersistEnum(ref shader);
            if (shader != Template.Shader)
            {
                throw new InvalidStateException();
            }

            ParticleSystemType type = default;
            reader.PersistEnum(ref type);
            if (type != Template.Type)
            {
                throw new InvalidStateException();
            }

            var texture = "";
            reader.PersistAsciiString(ref texture);

            RandomVariable angleX = default;
            reader.PersistRandomVariable(ref angleX);

            RandomVariable angleY = default;
            reader.PersistRandomVariable(ref angleY);

            RandomVariable angleZ = default;
            reader.PersistRandomVariable(ref angleZ);

            RandomVariable angularRateX = default;
            reader.PersistRandomVariable(ref angularRateX);

            RandomVariable angularRateY = default;
            reader.PersistRandomVariable(ref angularRateY);

            RandomVariable angularRateZ = default;
            reader.PersistRandomVariable(ref angularRateZ);

            RandomVariable angularDamping = default;
            reader.PersistRandomVariable(ref angularDamping);

            RandomVariable velocityDamping = default;
            reader.PersistRandomVariable(ref velocityDamping);

            RandomVariable lifetime = default;
            reader.PersistRandomVariable(ref lifetime);

            var systemLifetime = 0u;
            reader.PersistUInt32(ref systemLifetime);

            RandomVariable size = default;
            reader.PersistRandomVariable(ref size);

            RandomVariable startSizeRate = default;
            reader.PersistRandomVariable(ref startSizeRate);

            RandomVariable sizeRate = default;
            reader.PersistRandomVariable(ref sizeRate);

            RandomVariable sizeRateDamping = default;
            reader.PersistRandomVariable(ref sizeRateDamping);

            for (var i = 0; i < 8; i++)
            {
                RandomAlphaKeyframe keyframe = default;
                reader.PersistRandomAlphaKeyframe(ref keyframe);
            }
            for (var i = 0; i < 8; i++)
            {
                RgbColorKeyframe keyframe = default;
                reader.PersistRgbColorKeyframe(ref keyframe); // ColorKeyframes
            }

            RandomVariable colorScale = default;
            reader.PersistRandomVariable(ref colorScale);

            RandomVariable burstDelay = default;
            reader.PersistRandomVariable(ref burstDelay);

            RandomVariable burstCount = default;
            reader.PersistRandomVariable(ref burstCount);

            RandomVariable initialDelay = default;
            reader.PersistRandomVariable(ref initialDelay);

            Vector3 driftVelocity = default;
            reader.PersistVector3(ref driftVelocity);

            var gravity = 0.0f;
            reader.PersistSingle(ref gravity); // Gravity

            var slaveSystemName = "";
            reader.PersistAsciiString(ref slaveSystemName); // SlaveSystemName

            reader.SkipUnknownBytes(13);

            ParticleVelocityType velocityType = default;
            reader.PersistEnum(ref velocityType);
            var unknown10 = 0u;
            reader.PersistUInt32(ref unknown10);
            switch (velocityType)
            {
                case ParticleVelocityType.Ortho:
                    RandomVariable velocityOrthoX = default;
                    reader.PersistRandomVariable(ref velocityOrthoX);
                    RandomVariable velocityOrthoY = default;
                    reader.PersistRandomVariable(ref velocityOrthoY);
                    RandomVariable velocityOrthoZ = default;
                    reader.PersistRandomVariable(ref velocityOrthoZ);
                    break;
                case ParticleVelocityType.Spherical:
                    RandomVariable velocitySpherical = default;
                    reader.PersistRandomVariable(ref velocitySpherical);
                    break;
                case ParticleVelocityType.Hemispherical:
                    RandomVariable velocityHemispherical = default;
                    reader.PersistRandomVariable(ref velocityHemispherical);
                    break;
                case ParticleVelocityType.Cylindrical:
                    RandomVariable velocityCylindricalRadial = default;
                    reader.PersistRandomVariable(ref velocityCylindricalRadial);
                    RandomVariable velocityCylindricalNormal = default;
                    reader.PersistRandomVariable(ref velocityCylindricalNormal);
                    break;
                case ParticleVelocityType.Outward:
                    RandomVariable velocityOutward = default;
                    reader.PersistRandomVariable(ref velocityOutward);
                    RandomVariable velocityOutwardOther = default;
                    reader.PersistRandomVariable(ref velocityOutwardOther);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ParticleVolumeType volumeType = default;
            reader.PersistEnum(ref volumeType);
            switch (volumeType)
            {
                case ParticleVolumeType.Point:
                    break;
                case ParticleVolumeType.Line:
                    Vector3 lineStartPoint = default;
                    Vector3 lineEndPoint = default;
                    reader.PersistVector3(ref lineStartPoint);
                    reader.PersistVector3(ref lineEndPoint);
                    break;
                case ParticleVolumeType.Box:
                    Vector3 halfSize = default;
                    reader.PersistVector3(ref halfSize);
                    break;
                case ParticleVolumeType.Sphere:
                    var volumeSphereRadius = 0.0f;
                    reader.PersistSingle(ref volumeSphereRadius); // Interesting, value doesn't match ini file
                    break;
                case ParticleVolumeType.Cylinder:
                    var volumeCylinderRadius = 0.0f;
                    reader.PersistSingle(ref volumeCylinderRadius);
                    var volumeCylinderLength = 0.0f;
                    reader.PersistSingle(ref volumeCylinderLength);
                    break;
                default:
                    throw new NotImplementedException();
            }
            var unknown11 = 0u;
            reader.PersistUInt32(ref unknown11);
            ParticleSystemWindMotion windMotion = default;
            reader.PersistEnum(ref windMotion);
            var unknown12 = 0.0f;
            reader.PersistSingle(ref unknown12);
            var unknown13 = 0.0f;
            reader.PersistSingle(ref unknown13); // Almost same as WindAngleChangeMin
            var windAngleChangeMin = 0.0f;
            reader.PersistSingle(ref windAngleChangeMin);
            var windAngleChangeMax = 0.0f;
            reader.PersistSingle(ref windAngleChangeMax);
            var unknown14 = 0.0f;
            reader.PersistSingle(ref unknown14);
            var windPingPongStartAngleMin = 0.0f;
            reader.PersistSingle(ref windPingPongStartAngleMin);
            var windPingPongStartAngleMax = 0.0f;
            reader.PersistSingle(ref windPingPongStartAngleMax);
            var unknown15 = 0.0f;
            reader.PersistSingle(ref unknown15);
            var windPingPongEndAngleMin = 0.0f;
            reader.PersistSingle(ref windPingPongEndAngleMin);
            var windPingPongEndAngleMax = 0.0f;
            reader.PersistSingle(ref windPingPongEndAngleMax);

            var unknown16 = false;
            reader.PersistBoolean(ref unknown16);
        }
    }

    public enum ParticleSystemState
    {
        Inactive,
        Active,
        Finished,
        Dead
    }

    internal readonly struct ParticleColorKeyframe : IParticleKeyframe
    {
        public uint Time { get; }
        public readonly Vector3 Color;

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
    }

    internal interface IParticleKeyframe
    {
        uint Time { get; }
    }
}
