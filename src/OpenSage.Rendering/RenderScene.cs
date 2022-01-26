using System;
using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class RenderScene
{
    private readonly List<RenderableObject> _renderables = new();

    private readonly RenderList _forwardPassList = new();
    private readonly RenderList _shadowPassList = new();

    public void AddObject(RenderableObject renderable)
    {
        _renderables.Add(renderable);

        // TODO: Extract child objects and store in appropriate render buckets (opaque / transparent / etc.)
    }
}

public abstract class RenderableObject
{
    public readonly List<RenderableObject> ChildObjects = new();

    public abstract SurfaceType SurfaceType { get; }

    public abstract ulong RenderKey { get; } // ShaderSet, Pipeline, Material, SortPriority

    //public abstract int SortPriority { get; } // Needs to be lower for Road than for ParticleSystem

    public abstract bool CastsShadow { get; }
}

public readonly struct RenderKey
{
    private readonly ulong _key;

    // TODO: Also material constants?

    public RenderKey(
        SurfaceType surfaceType,
        ShaderSet shaderSet,
        Pipeline pipeline)
        // TODO: Also material constants?
    {
        // TODO
        _key = 0;
    }
}

//public sealed class RenderContext
//{
//}

public sealed class Material : DisposableBase
{
    public readonly byte Id;
    public readonly ShaderSet ShaderSet;
    public readonly Pipeline Pipeline;
    public readonly ResourceSet MaterialResourceSet;

    public Material(
        ShaderSet shaderSet,
        Pipeline pipeline,
        ResourceSet materialResourceSet)
    {
        Id = shaderSet.GetNextMaterialId();

        ShaderSet = shaderSet;
        Pipeline = pipeline;
        MaterialResourceSet = materialResourceSet;
    }

    public ulong RenderKey { get; } // TODO
}

public sealed record MaterialPass(Material ForwardPass, Material ShadowPass);

public enum SurfaceType
{
    Opaque,
    Transparent,
}

public enum RenderBucketType
{
    Terrain,
    Road,
    Opaque,
    Transparent,
    Water,
}

internal sealed class RenderList
{
    public readonly List<RenderableObject> Opaque = new();
    public readonly List<RenderableObject> Transparent = new();
}
