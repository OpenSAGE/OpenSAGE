using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderLibrary : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, ShaderSet> _shaderSets;

        public ShaderSet FixedFunction { get; }
        public ShaderSet Particle { get; }
        public ShaderSet Sprite { get; }
        public ShaderSet Terrain { get; }
        public ShaderSet Road { get; }
        public ShaderSet Water { get; }

        public ShaderSet MeshDepth { get; }

        public ShaderLibrary(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _shaderSets = new Dictionary<string, ShaderSet>();

            FixedFunction = AddDisposable(new ShaderSet(
                graphicsDevice,
                "FixedFunction",
                new GlobalResourceSetIndices(0u, LightingType.Object, 1u, 2u, 3u, 7u),
                MeshTypes.MeshVertex.VertexDescriptors));

            Particle = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Particle",
                new GlobalResourceSetIndices(0u, LightingType.None, null, null, null, null),
                ParticleTypes.ParticleVertex.VertexDescriptor));

            Sprite = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Sprite",
                new GlobalResourceSetIndices(null, LightingType.None, null, null, null, null),
                SpriteTypes.SpriteVertex.VertexDescriptor));

            Terrain = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Terrain",
                new GlobalResourceSetIndices(0u, LightingType.Terrain, 1u, 2u, 3u, null),
                TerrainTypes.TerrainVertex.VertexDescriptor));

            Road = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Road",
                new GlobalResourceSetIndices(0u, LightingType.Terrain, 1u, 2u, 3u, null),
                RoadTypes.RoadVertex.VertexDescriptor));

            Water = AddDisposable(new ShaderSet(
                graphicsDevice,
                "Water",
                new GlobalResourceSetIndices(0u, LightingType.Terrain, 1u, 2u, 3u, null),
                WaterTypes.WaterVertex.VertexDescriptor));

            MeshDepth = AddDisposable(new ShaderSet(
                graphicsDevice,
                "MeshDepth",
                new GlobalResourceSetIndices(0u, LightingType.None, null, null, null, 2u),
                MeshTypes.MeshVertex.VertexDescriptors));
        }

        public ShaderSet GetMeshShaderSet(string name)
        {
            if (!_shaderSets.TryGetValue(name, out var shaderSet))
            {
                _shaderSets[name] = shaderSet = AddDisposable(new ShaderSet(
                    _graphicsDevice,
                    name,
                    new GlobalResourceSetIndices(0u, LightingType.Object, 1u, 2u, 3u, 6u),
                    MeshTypes.MeshVertex.VertexDescriptors));
            }
            return shaderSet;
        }
    }
}
