using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public sealed class GuiComponent : RenderableComponent
    {
        public GuiWindow Window { get; set; }

        internal override BoundingBox LocalBoundingBox => default;

        internal override bool IsAlwaysVisible => true;

        internal override void BuildRenderList(RenderList renderList)
        {
            Game.Gui.AddRenderItem(renderList, this);
        }
    }
}
