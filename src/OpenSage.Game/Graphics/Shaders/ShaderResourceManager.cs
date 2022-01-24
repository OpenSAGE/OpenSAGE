using System.Collections.Generic;
using OpenSage.Diagnostics;
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
            StandardGraphicsResources standardGraphicsResources)
        {
            using (GameTrace.TraceDurationEvent("ShaderResourceManager()"))
            {
                Global = AddDisposable(new GlobalShaderResources(graphicsDevice));
                Mesh = AddDisposable(new MeshShaderResources(graphicsDevice));

                RadiusCursor = AddDisposable(new RadiusCursorDecalShaderResources(graphicsDevice, standardGraphicsResources.Aniso4xClampSampler));

                FixedFunction = AddDisposable(new FixedFunctionShaderResources(graphicsDevice, Global, Mesh));
                MeshDepth = AddDisposable(new MeshDepthShaderResources(graphicsDevice, Global, Mesh));
                Particle = AddDisposable(new ParticleShaderResources(graphicsDevice, Global));
                Road = AddDisposable(new RoadShaderResources(graphicsDevice, Global));
                Sprite = AddDisposable(new SpriteShaderResources(graphicsDevice));
                Terrain = AddDisposable(new TerrainShaderResources(graphicsDevice, Global, RadiusCursor));
                Water = AddDisposable(new WaterShaderResources(graphicsDevice, Global));

                _shaderMaterialResources = new Dictionary<string, ShaderMaterialShaderResources>
                {
                    { "NormalMapped", AddDisposable(new NormalMappedShaderResources(graphicsDevice, Global, Mesh)) },
                    { "Simple", AddDisposable(new SimpleShaderResources(graphicsDevice, Global, Mesh)) }
                };
            }
        }

        public ShaderMaterialShaderResources GetShaderMaterialResources(string name)
        {
            return _shaderMaterialResources[name];
        }
    }
}
