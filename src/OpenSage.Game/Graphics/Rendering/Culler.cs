using System.Collections.Generic;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class Culler
    {
        public static void Cull(List<RenderItem> items, List<RenderItem> culledItems, RenderContext context)
        {
            foreach (var renderItem in items)
            {
                if (renderItem.CullFlags.HasFlag(CullFlags.AlwaysVisible))
                {
                    culledItems.Add(renderItem);
                    continue;
                }

                if (!BoundingFrustum.Intersects(context.Camera.BoundingFrustum, renderItem.BoundingBox))
                {
                    continue;
                }

                culledItems.Add(renderItem);
            }
        }
    }
}
