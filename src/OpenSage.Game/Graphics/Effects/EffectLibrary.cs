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
        public Effect Road { get; }
        public Effect Water { get; }

        public Effect MeshDepthFixedFunction { get; }
        public Effect MeshDepthShaderMaterial { get; }

        public EffectLibrary(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _effects = new Dictionary<string, Effect>();

            FixedFunction = AddDisposable(new Effect(
                graphicsDevice,
                "FixedFunction",
                PrimitiveTopology.TriangleList,
                MeshVertex.VertexDescriptors));

            Particle = AddDisposable(new Effect(
                graphicsDevice,
                "Particle",
                PrimitiveTopology.TriangleList,
                ParticleVertex.VertexDescriptor));

            Sprite = AddDisposable(new Effect(
                graphicsDevice,
                "Sprite",
                PrimitiveTopology.TriangleList,
                SpriteVertex.VertexDescriptor));

            Terrain = AddDisposable(new Effect(
                graphicsDevice,
                "Terrain",
                PrimitiveTopology.TriangleList, // TODO: Use TriangleStrip
                TerrainVertex.VertexDescriptor));

            Road = AddDisposable(new Effect(
                graphicsDevice,
                "Road",
                PrimitiveTopology.TriangleList,
                RoadVertex.VertexDescriptor));

            Water = AddDisposable(new Effect(
                graphicsDevice,
                "Water",
                PrimitiveTopology.TriangleList,
                WaterVertex.VertexDescriptor));

            MeshDepthFixedFunction = AddDisposable(new Effect(
                graphicsDevice,
                "MeshDepth",
                PrimitiveTopology.TriangleList,
                MeshVertex.VertexDescriptors));

            MeshDepthShaderMaterial = AddDisposable(new Effect(
                graphicsDevice,
                "MeshDepth",
                PrimitiveTopology.TriangleStrip,
                MeshVertex.VertexDescriptors));
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
                    PrimitiveTopology.TriangleStrip,
                    vertexDescriptors));
            }
            return effect;
        }
    }
}
