using System.Collections.Generic;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class RenderScene
{
    public readonly List<RenderObject> Objects = new();

    private readonly List<RenderObject> _visibleObjects = new();
    private readonly List<(RenderObject, Material)> _filteredObjects = new();

    public readonly HashSet<RenderBucketType> HiddenRenderBuckets = new();

    public void SetRenderBucketVisibility(RenderBucketType renderBucket, bool visible)
    {
        if (visible)
        {
            HiddenRenderBuckets.Remove(renderBucket);
        }
        else
        {
            HiddenRenderBuckets.Add(renderBucket);
        }
    }

    public void Render(
        CommandList commandList,
        ResourceSet globalResourceSet,
        ResourceSet passResourceSet)
    {
        // TODO
        //DoShadowPass();

        // Draw opaque objects.
        DoRenderPass(
            "Forward",
            new RenderBucketRange((int)RenderBucketType.Terrain, (int)RenderBucketType.Opaque),
            commandList,
            globalResourceSet,
            passResourceSet);

        // Draw transparent objects.
        DoRenderPass(
            "Forward",
            new RenderBucketRange((int)RenderBucketType.Transparent, (int)RenderBucketType.Transparent),
            commandList,
            globalResourceSet,
            passResourceSet);
    }

    private readonly record struct RenderBucketRange(int Min, int Max);

    private void DoRenderPass(
        string passName,
        RenderBucketRange renderBucketRange,
        CommandList commandList,
        ResourceSet globalResourceSet,
        ResourceSet passResourceSet)
    {
        commandList.PushDebugGroup(passName);

        // Find objects that are visible to this camera.
        // TODO
        _visibleObjects.Clear();
        _visibleObjects.AddRange(Objects);

        // Filter to only the objects that have a material that participates in this pass.
        _filteredObjects.Clear();
        foreach (var renderObject in _visibleObjects)
        {
            if (!renderObject.MaterialPass.Passes.TryGetValue(passName, out var material))
            {
                continue;
            }

            if (HiddenRenderBuckets.Contains(renderObject.MaterialPass.RenderBucket))
            {
                continue;
            }

            if ((int)renderObject.MaterialPass.RenderBucket >= renderBucketRange.Min
                && (int)renderObject.MaterialPass.RenderBucket <= renderBucketRange.Max)
            {
                _filteredObjects.Add((renderObject, material));
            }
        }

        // Sort based on render bucket and material.
        _filteredObjects.Sort(static (x, y) =>
        {
            var result = ((int)x.Item1.MaterialPass.RenderBucket).CompareTo((int)y.Item1.MaterialPass.RenderBucket);
            if (result != 0)
            {
                return result;
            }

            return x.Item2.RenderKey.CompareTo(y.Item2.RenderKey);

            // TODO: Sort transparent objects back-to-front.
        });

        foreach (var renderObject in _filteredObjects)
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
