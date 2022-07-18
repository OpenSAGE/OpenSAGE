using Veldrid;

namespace OpenSage.Rendering;

public sealed class Material : DisposableBase
{
    public readonly ushort Id;
    public readonly ShaderSet ShaderSet;
    public readonly Pipeline Pipeline;
    public readonly ResourceSet MaterialResourceSet;

    public readonly int RenderKey;

    public Material(
        ShaderSet shaderSet,
        Pipeline pipeline,
        ResourceSet materialResourceSet,
        SurfaceType surfaceType)
    {
        Id = shaderSet.GetNextMaterialId();

        ShaderSet = shaderSet;
        Pipeline = pipeline;
        MaterialResourceSet = materialResourceSet;

        // Bit 31: SurfaceType
        RenderKey |= ((int)surfaceType) << 31;

        // Bit 23-30: ShaderSet
        RenderKey |= (ShaderSet.Id << 23);

        // Bit 15-22: Material
        RenderKey |= (Id) << 15;
    }
}

public enum SurfaceType : byte
{
    Opaque = 0,
    Transparent = 1,
}
