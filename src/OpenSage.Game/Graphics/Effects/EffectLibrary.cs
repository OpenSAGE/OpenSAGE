using System.Collections.Generic;
using LL.Graphics3D;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Terrain;

namespace OpenSage.Graphics.Effects
{
    public sealed class EffectLibrary : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, Effect> _effects;

        public Effect FixedFunction { get; }
        public Effect Particle { get; }
        public Effect Sprite { get; }
        public Effect Terrain { get; }

        public EffectLibrary(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _effects = new Dictionary<string, Effect>();

            FixedFunction = AddDisposable(new Effect(
                graphicsDevice,
                "FixedFunction",
                MeshVertex.VertexDescriptor));

            Particle = AddDisposable(new Effect(
                graphicsDevice,
                "Particle",
                ParticleVertex.VertexDescriptor));

            Sprite = AddDisposable(new Effect(
                graphicsDevice,
                "Sprite",
                SpriteVertex.VertexDescriptor));

            Terrain = AddDisposable(new Effect(
                graphicsDevice,
                "Terrain",
                TerrainVertex.VertexDescriptor));
        }

        public Effect GetEffect(string name, VertexDescriptor vertexDescriptor)
        {
            if (!_effects.TryGetValue(name, out var effect))
            {
                _effects[name] = effect = AddDisposable(new Effect(
                    _graphicsDevice,
                    name,
                    vertexDescriptor));
            }
            return effect;
        }
    }
}
