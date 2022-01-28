using System.Collections.Generic;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class RenderScene
{
    private readonly List<RenderObject> _renderObjects = new();

    private readonly RenderList _forwardPassList = new();
    private readonly RenderList _shadowPassList = new();

    public void AddObject(RenderObject renderObject)
    {
        _renderObjects.Add(renderObject);

        _forwardPassList.AddObject(renderObject, renderObject.MaterialPass.ForwardPass);

        if (renderObject.MaterialPass.ShadowPass != null)
        {
            _shadowPassList.AddObject(renderObject, renderObject.MaterialPass.ShadowPass);
        }

        // TODO: Extract child objects and store in appropriate render buckets (opaque / transparent / etc.)
    }

    public void RemoveObject(RenderObject renderObject)
    {
        _renderObjects.Remove(renderObject);

        _forwardPassList.RemoveObject(renderObject, renderObject.MaterialPass.ForwardPass);

        if (renderObject.MaterialPass.ShadowPass != null)
        {
            _shadowPassList.RemoveObject(renderObject, renderObject.MaterialPass.ShadowPass);
        }
    }

    public void Render()
    {
        // TODO: Draw shadow map using _shadowPassList.
        //DoShadowPass();

        DoForwardPass();
    }

    private void DoForwardPass()
    {
        //_forwardPassList.Cull();
    }
}

public abstract class RenderObject : DisposableBase
{
    public abstract MaterialPass MaterialPass { get; }

    //public readonly List<RenderableObject> ChildObjects = new();

    public abstract AxisAlignedBoundingBox BoundingBox { get; }

    //public abstract Material Material { get; }

    //public abstract SurfaceType SurfaceType { get; }

    //public abstract ulong RenderKey { get; } // ShaderSet, Pipeline, Material, SortPriority

    //public abstract int SortPriority { get; } // Needs to be lower for Road than for ParticleSystem

    //public abstract bool CastsShadow { get; }

    public abstract void Render();
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
    public readonly List<(RenderObject, Material)> AllObjects = new();

    public readonly List<(RenderObject, Material)> CulledObjects = new();

    public void AddObject(RenderObject renderObject, Material material)
    {
        AllObjects.Add((renderObject, material));

        // TODO: Use something better like https://www.jacksondunstan.com/articles/3189
        AllObjects.Sort((x, y) => x.Item2.RenderKey.CompareTo(y.Item2.RenderKey));
    }

    public void RemoveObject(RenderObject renderObject, Material material)
    {
        // TODO: Do something faster.
        var index = AllObjects.IndexOf((renderObject, material));

        AllObjects.RemoveAt(index);
    }
}
