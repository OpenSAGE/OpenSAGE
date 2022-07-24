using System.Collections.Generic;
using OpenSage.Diagnostics;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderResourceManager : DisposableBase
    {
        public readonly GlobalShaderResources Global;

        public readonly RadiusCursorDecalShaderResources RadiusCursor;

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
                
                RadiusCursor = AddDisposable(new RadiusCursorDecalShaderResources(graphicsDevice, standardGraphicsResources.Aniso4xClampSampler));

                Particle = AddDisposable(new ParticleShaderResources(shaderSetStore));
                Road = AddDisposable(new RoadShaderResources(shaderSetStore));
                Sprite = AddDisposable(new SpriteShaderResources(shaderSetStore));
                Terrain = AddDisposable(new TerrainShaderResources(shaderSetStore));
                Water = AddDisposable(new WaterShaderResources(shaderSetStore));
            }
        }
    }
}
