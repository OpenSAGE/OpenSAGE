using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class RenderScene
{
    public readonly List<RenderObject> Objects = new();

    private readonly List<(RenderObject, Material)> _filteredObjects = new();

    private readonly Culler _culler = new();

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
        ResourceSet passResourceSet,
        BoundingFrustum cameraFrustum,
        in Plane? clippingPlane1 = null,
        in Plane? clippingPlane2 = null)
    {
        // TODO
        //DoShadowPass();

        // Draw opaque objects.
        DoRenderPass(
            "Forward",
            new RenderBucketRange((int)RenderBucketType.Terrain, (int)RenderBucketType.Opaque),
            commandList,
            globalResourceSet,
            passResourceSet,
            cameraFrustum,
            clippingPlane1,
            clippingPlane2);

        // Draw transparent objects.
        DoRenderPass(
            "Forward",
            new RenderBucketRange((int)RenderBucketType.Transparent, (int)RenderBucketType.Transparent),
            commandList,
            globalResourceSet,
            passResourceSet,
            cameraFrustum,
            clippingPlane1,
            clippingPlane2);
    }

    private readonly record struct RenderBucketRange(int Min, int Max);

    private void DoRenderPass(
        string passName,
        RenderBucketRange renderBucketRange,
        CommandList commandList,
        ResourceSet globalResourceSet,
        ResourceSet passResourceSet,
        BoundingFrustum cameraFrustum,
        in Plane? clippingPlane1 = null,
        in Plane? clippingPlane2 = null)
    {
        commandList.PushDebugGroup(passName);

        // Find objects that are visible to this camera.
        _culler.Cull(Objects, cameraFrustum, clippingPlane1, clippingPlane2, out var culledObjects);

        // Filter to only the objects that have a material that participates in this pass.
        _filteredObjects.Clear();
        foreach (var renderObject in culledObjects)
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

public sealed class Culler
{
    private const int ParallelCullingBatchSize = 128;
    private const double GrowthFactor = 1.5;

    private readonly List<RenderObject> _culledObjects = new();

    // An array of flags indicating if a render item should be included in the culling set.
    private bool[] _culled = Array.Empty<bool>();

    public void Cull(
        List<RenderObject> renderObjects,
        in BoundingFrustum cameraFrustum,
        in Plane? clippingPlane1,
        in Plane? clippingPlane2,
        out List<RenderObject> culledObjects)
    {
        if (renderObjects.Count > _culled.Length)
        {
            var newCapacity = (int)(renderObjects.Count * GrowthFactor);
            Array.Resize(ref _culled, newCapacity);
        }

        // We need a copy of cameraFrustum, as we can't send in parameters to closures. 
        var frustum = cameraFrustum;
        // We need a copy of clippingPlane, as we can't send in parameters to closures. 
        var clip1 = clippingPlane1;
        var clip2 = clippingPlane2;

        void Cull(int i, in Plane clippingPlane)
        {
            _culled[i] = _culled[i] && renderObjects[i].BoundingBox.Intersects(clippingPlane) != PlaneIntersectionType.Back;
        }

        // Perform culling using the thread pool, in batches of batchSize.
        Parallel.ForEach(Partitioner.Create(0, renderObjects.Count, ParallelCullingBatchSize), range =>
        {
            var (start, end) = range;
            for (var i = start; i < end; i++)
            {
                _culled[i] = frustum.Intersects(renderObjects[i].BoundingBox);

                if (clip1 != null)
                {
                    Cull(i, clip1.Value);
                }

                if (clip2 != null)
                {
                    Cull(i, clip2.Value);
                }
            }
        });

        _culledObjects.Clear();

        for (var i = 0; i < renderObjects.Count; i++)
        {
            if (_culled[i])
            {
                _culledObjects.Add(renderObjects[i]);
            }
        }

        culledObjects = _culledObjects;
    }
}
