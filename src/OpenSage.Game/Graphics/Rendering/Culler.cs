using System.Collections.Generic;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class Culler
    {
        public void Cull(List<RenderItem> items, RenderContext context)
        {
            foreach (var renderItem in items)
            {
                renderItem.Visible = true;

                if (renderItem.Renderable.IsAlwaysVisible)
                {
                    continue;
                }

                if (!renderItem.Renderable.Entity.VisibleInHierarchy)
                {
                    renderItem.Visible = false;
                    continue;
                }

                if (!context.Camera.BoundingFrustum.Intersects(renderItem.Renderable.BoundingBox))
                {
                    renderItem.Visible = false;
                    continue;
                }
            }
        }
    }
}
