using System.Collections.Generic;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class RenderBucket
{
    private readonly List<RenderObject> _renderObjects = new();

    private readonly RenderList _forwardPassList = new("Forward Pass");
    private readonly RenderList _shadowPassList = new("Shadow Pass");

    public readonly string Name;
    public readonly int Priority;

    public bool Visible = true;

    internal RenderBucket(string name, int priority)
    {
        Name = name;
        Priority = priority;
    }

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

    internal void DoRenderPass(
        RenderPass renderPass,
        CommandList commandList,
        ResourceSet
        globalResourceSet,
        ResourceSet passResourceSet)
    {
        if (!Visible)
        {
            return;
        }

        var renderList = renderPass == RenderPass.Forward
            ? _forwardPassList
            : _shadowPassList;

        commandList.PushDebugGroup(Name);

        renderList.Cull();

        foreach (var renderObject in renderList.CulledObjects)
        {
            commandList.PushDebugGroup(renderObject.Item1.DebugName);

            commandList.SetPipeline(renderObject.Item2.Pipeline);

            commandList.SetGraphicsResourceSet(0, globalResourceSet);

            if (passResourceSet != null)
            {
                commandList.SetGraphicsResourceSet(1, passResourceSet);
            }

            if (renderObject.Item2.MaterialResourceSet != null)
            {
                commandList.SetGraphicsResourceSet(2, renderObject.Item2.MaterialResourceSet);
            }

            renderObject.Item1.Render(commandList);

            commandList.PopDebugGroup();
        }

        commandList.PopDebugGroup();
    }
}

public sealed class RenderScene
{
    private readonly List<RenderBucket> _renderBuckets = new();

    public RenderBucket GetRenderBucket(string name)
    {
        var result = GetRenderBucketImpl(name);

        if (result == null)
        {
            throw new System.InvalidOperationException();
        }

        return result;
    }

    private RenderBucket GetRenderBucketImpl(string name)
    {
        foreach (var renderBucket in _renderBuckets)
        {
            if (renderBucket.Name == name)
            {
                return renderBucket;
            }
        }

        return null;
    }

    public RenderBucket CreateRenderBucket(string name, int priority)
    {
        var existingRenderBucket = GetRenderBucketImpl(name);
        if (existingRenderBucket != null)
        {
            throw new System.InvalidOperationException();
        }

        var newRenderBucket = new RenderBucket(name, priority);
        _renderBuckets.Add(newRenderBucket);

        // TODO: Use something better like https://www.jacksondunstan.com/articles/3189
        _renderBuckets.Sort((x, y) => x.Priority.CompareTo(y.Priority));

        return newRenderBucket;
    }

    public void Render(CommandList commandList, ResourceSet globalResourceSet, ResourceSet passResourceSet)
    {
        // TODO: Draw shadow map using _shadowPassList.
        //DoShadowPass();

        DoRenderPass(RenderPass.Forward, commandList, globalResourceSet, passResourceSet);
    }

    private void DoRenderPass(
        RenderPass renderPass,
        CommandList commandList,
        ResourceSet globalResourceSet,
        ResourceSet passResourceSet)
    {
        commandList.PushDebugGroup(renderPass.ToString());

        foreach (var renderBucket in _renderBuckets)
        {
            renderBucket.DoRenderPass(
                renderPass,
                commandList,
                globalResourceSet,
                passResourceSet);
        }

        commandList.PopDebugGroup();
    }
}

public abstract class RenderObject : DisposableBase
{
    public abstract string DebugName { get; }

    public abstract MaterialPass MaterialPass { get; }

    //public readonly List<RenderableObject> ChildObjects = new();

    public abstract AxisAlignedBoundingBox BoundingBox { get; }

    //public abstract Material Material { get; }

    //public abstract SurfaceType SurfaceType { get; }

    //public abstract ulong RenderKey { get; } // ShaderSet, Pipeline, Material, SortPriority

    //public abstract int SortPriority { get; } // Needs to be lower for Road than for ParticleSystem

    //public abstract bool CastsShadow { get; }

    public abstract void Render(CommandList commandList);
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
    public readonly string Name;

    public readonly List<(RenderObject, Material)> AllObjects = new();

    public readonly List<(RenderObject, Material)> CulledObjects = new();

    public RenderList(string name)
    {
        Name = name;
    }

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

    public void Cull()
    {
        // TODO

        CulledObjects.Clear();

        CulledObjects.AddRange(AllObjects);
    }
}

internal enum RenderPass
{
    Forward,
    Shadow,
}
