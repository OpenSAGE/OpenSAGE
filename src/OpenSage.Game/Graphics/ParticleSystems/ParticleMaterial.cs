using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Rendering;

namespace OpenSage.Graphics.ParticleSystems;

internal static class ShaderSetStoreExtensions
{
    public static ParticleShaderResources GetParticleShaderSet(this ShaderSetStore store)
    {
        return store.GetShaderSet(() => new ParticleShaderResources(store));
    }
}

public struct ParticleRenderItemConstantsVS
{
    public Matrix4x4 WorldMatrix;
}
