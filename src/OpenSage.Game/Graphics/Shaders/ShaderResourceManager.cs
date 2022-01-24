using System.Collections.Generic;
using OpenSage.Diagnostics;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderResourceManager : DisposableBase
    {
        private readonly Dictionary<string, ShaderMaterialShaderResources> _shaderMaterialResources;

        public readonly GlobalShaderResources Global;
        public readonly MeshShaderResources Mesh;

        public readonly RadiusCursorDecalShaderResources RadiusCursor;

        public readonly FixedFunctionShaderResources FixedFunction;
        public readonly MeshDepthShaderResources MeshDepth;
        public readonly ParticleShaderResources Particle;
        public readonly RoadShaderResources Road;
        public readonly SpriteShaderResources Sprite;
        public readonly TerrainShaderResources Terrain;
        public readonly WaterShaderResources Water;

        public ShaderResourceManager(
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            ShaderSetStore shaderSetStore)
        {
            using (GameTrace.TraceDurationEvent("ShaderResourceManager()"))
            {
                Global = AddDisposable(new GlobalShaderResources(graphicsDevice));
                Mesh = AddDisposable(new MeshShaderResources(graphicsDevice));

                RadiusCursor = AddDisposable(new RadiusCursorDecalShaderResources(graphicsDevice, standardGraphicsResources.Aniso4xClampSampler));

                FixedFunction = AddDisposable(new FixedFunctionShaderResources(shaderSetStore));
                MeshDepth = AddDisposable(new MeshDepthShaderResources(shaderSetStore));
                Particle = AddDisposable(new ParticleShaderResources(shaderSetStore));
                Road = AddDisposable(new RoadShaderResources(shaderSetStore));
                Sprite = AddDisposable(new SpriteShaderResources(shaderSetStore));
                Terrain = AddDisposable(new TerrainShaderResources(shaderSetStore));
                Water = AddDisposable(new WaterShaderResources(shaderSetStore));

                _shaderMaterialResources = new Dictionary<string, ShaderMaterialShaderResources>
                {
                    { "NormalMapped", AddDisposable(new NormalMappedShaderResources(shaderSetStore)) },
                    { "Simple", AddDisposable(new SimpleShaderResources(shaderSetStore)) }
                };
            }
        }

        public ShaderMaterialShaderResources GetShaderMaterialResources(string name)
        {
            return _shaderMaterialResources[name];
        }
    }
}
