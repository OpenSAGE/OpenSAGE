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
        ResourceSet materialResourceSet)
    {
        Id = shaderSet.GetNextMaterialId();

        ShaderSet = shaderSet;
        Pipeline = pipeline;
        MaterialResourceSet = materialResourceSet;

        // Bit 24-31: ShaderSet
        RenderKey |= (ShaderSet.Id << 24);

        // Bit 16-23: Material
        RenderKey |= (Id) << 16;
    }
}
