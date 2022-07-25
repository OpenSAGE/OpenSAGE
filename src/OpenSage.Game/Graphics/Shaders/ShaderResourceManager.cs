using OpenSage.Core.Graphics;
using OpenSage.Diagnostics;
using OpenSage.Rendering;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderResourceManager : DisposableBase
    {
        public readonly GlobalShaderResources Global;

        public readonly RadiusCursorDecalShaderResources RadiusCursor;

        public readonly ParticleShaderResources Particle;
        public readonly RoadShaderResources Road;
        public readonly TerrainShaderResources Terrain;
        public readonly WaterShaderResources Water;

        public ShaderResourceManager(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore)
        {
            using (GameTrace.TraceDurationEvent("ShaderResourceManager()"))
            {
                Global = AddDisposable(new GlobalShaderResources(graphicsDeviceManager.GraphicsDevice));
                
                RadiusCursor = AddDisposable(new RadiusCursorDecalShaderResources(graphicsDeviceManager));

                Particle = AddDisposable(new ParticleShaderResources(shaderSetStore));
                Road = AddDisposable(new RoadShaderResources(shaderSetStore));
                Terrain = AddDisposable(new TerrainShaderResources(shaderSetStore));
                Water = AddDisposable(new WaterShaderResources(shaderSetStore));
            }
        }
    }
}
