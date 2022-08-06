using OpenSage.Graphics.Shaders;
using OpenSage.Rendering;

namespace OpenSage.Graphics.Rendering.Water;

internal static class WaterShaderExtensions
{
    public static WaterShaderResources GetWaterShaderResources(this ShaderSetStore shaderSetStore)
    {
        return shaderSetStore.GetShaderSet(() => new WaterShaderResources(shaderSetStore));
    }
}
