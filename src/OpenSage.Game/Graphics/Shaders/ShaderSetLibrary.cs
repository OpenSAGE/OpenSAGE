using System.Collections.Generic;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Terrain;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderSetLibrary : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, ShaderSet> _shaderSets;

        public ShaderSet FixedFunction { get; }
        public ShaderSet Particle { get; }
        public ShaderSet Sprite { get; }
        public ShaderSet Terrain { get; }
        public ShaderSet Road { get; }
        public ShaderSet Water { get; }

        public ShaderSet MeshDepthFixedFunction { get; }
        public ShaderSet MeshDepthShaderMaterial { get; }

        public ShaderSetLibrary(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _shaderSets = new Dictionary<string, ShaderSet>();

            FixedFunction = AddDisposable(new ShaderSet(
                graphicsDevice,
                "FixedFunction",
                MeshVertex.VertexDescriptors));

            Particle = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Particle",
                ParticleVertex.VertexDescriptor));

            Sprite = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Sprite",
                SpriteVertex.VertexDescriptor));

            Terrain = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Terrain",
                TerrainVertex.VertexDescriptor));

            Road = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Road",
                RoadVertex.VertexDescriptor));

            Water = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Water",
                WaterVertex.VertexDescriptor));

            MeshDepthFixedFunction = AddDisposable(new ShaderSet(
                graphicsDevice,
                "MeshDepth",
                MeshVertex.VertexDescriptors));

            MeshDepthShaderMaterial = AddDisposable(new ShaderSet(
                graphicsDevice,
                "MeshDepth",
                MeshVertex.VertexDescriptors));
        }

        public ShaderSet GetShaderSet(
            string name,
            VertexLayoutDescription[] vertexDescriptors)
        {
            if (!_shaderSets.TryGetValue(name, out var shaderSet))
            {
                _shaderSets[name] = shaderSet = AddDisposable(new ShaderSet(
                    _graphicsDevice,
                    name,
                    vertexDescriptors));
            }
            return shaderSet;
        }
    }
}
