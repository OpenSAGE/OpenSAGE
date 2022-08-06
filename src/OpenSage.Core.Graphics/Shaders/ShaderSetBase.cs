using System;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders;

public abstract class ShaderSetBase : ShaderSet
{
    public ShaderSetBase(
        ShaderSetStore store,
        string shaderName,
        params VertexLayoutDescription[] vertexDescriptors)
        : base(store, Type.GetType("OpenSage.SageGame, OpenSage.Game").Assembly, shaderName, vertexDescriptors)
    {

    }
}
