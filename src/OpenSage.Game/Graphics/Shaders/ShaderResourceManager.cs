using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderResourceManager : DisposableBase
    {
        private readonly Dictionary<string, ShaderMaterialShaderResources> _shaderMaterialResources;

        public readonly FixedFunctionShaderResources FixedFunction;
        public readonly MeshShaderResources Mesh;
        public readonly MeshDepthShaderResources MeshDepth;
        public readonly ParticleShaderResources Particle;
        public readonly RoadShaderResources Road;
        public readonly SpriteShaderResources Sprite;
        public readonly TerrainShaderResources Terrain;
        public readonly WaterShaderResources Water;

        public ShaderResourceManager(GraphicsDevice graphicsDevice)
        {
            Mesh = AddDisposable(new MeshShaderResources(graphicsDevice));
            FixedFunction = AddDisposable(new FixedFunctionShaderResources(graphicsDevice));
            MeshDepth = AddDisposable(new MeshDepthShaderResources(graphicsDevice));
            Particle = AddDisposable(new ParticleShaderResources(graphicsDevice));
            Road = AddDisposable(new RoadShaderResources(graphicsDevice));
            Sprite = AddDisposable(new SpriteShaderResources(graphicsDevice));
            Terrain = AddDisposable(new TerrainShaderResources(graphicsDevice));
            Water = AddDisposable(new WaterShaderResources(graphicsDevice));

            _shaderMaterialResources = new Dictionary<string, ShaderMaterialShaderResources>
            {
                { "NormalMapped", AddDisposable(new NormalMappedShaderResources(graphicsDevice)) },
                { "Simple", AddDisposable(new SimpleShaderResources(graphicsDevice)) }
            };
        }

        public ShaderMaterialShaderResources GetShaderMaterialResources(string name)
        {
            return _shaderMaterialResources[name];
        }
    }
}
