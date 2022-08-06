using System.Collections.Generic;

namespace OpenSage.Rendering;

public sealed class MaterialPass
{
    public readonly RenderBucketType RenderBucket;

    public readonly Dictionary<string, Material> Passes = new();

    public MaterialPass(RenderBucketType renderBucket)
    {
        RenderBucket = renderBucket;
    }
}
