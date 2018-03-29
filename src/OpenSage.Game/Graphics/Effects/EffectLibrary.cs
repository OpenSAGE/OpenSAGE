using System.Collections.Generic;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Terrain;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class EffectLibrary : DisposableBase
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
                MeshVertex.VertexDescriptors,
                true));

            Particle = AddDisposable(new Effect(
                graphicsDevice,
                "Particle",
                ParticleVertex.VertexDescriptor,
                true));

            Sprite = AddDisposable(new Effect(
                graphicsDevice,
                "Sprite",
                SpriteVertex.VertexDescriptor,
                true));

            Terrain = AddDisposable(new Effect(
                graphicsDevice,
                "Terrain",
                TerrainVertex.VertexDescriptor));
        }

        public Effect GetEffect(
            string name,
            VertexLayoutDescription[] vertexDescriptors)
        {
            if (!_effects.TryGetValue(name, out var effect))
            {
                _effects[name] = effect = AddDisposable(new Effect(
                    _graphicsDevice,
                    name,
                    vertexDescriptors));
            }
            return effect;
        }
    }
}
