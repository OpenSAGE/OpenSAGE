using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders;

internal abstract class ShaderSetBase : ShaderSet
{
    public ShaderSetBase(
        ShaderSetStore store,
        string shaderName,
        params VertexLayoutDescription[] vertexDescriptors)
        : base(store, typeof(ShaderSetBase).Assembly, shaderName, vertexDescriptors)
    {

    }
}
