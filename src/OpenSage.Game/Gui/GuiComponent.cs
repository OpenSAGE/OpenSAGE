using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui.Elements;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public sealed class GuiComponent : RenderableComponent
    {
        public Window RootWindow { get; set; }

        internal override BoundingBox LocalBoundingBox => default(BoundingBox);

        internal override bool IsAlwaysVisible => true;

        internal override void BuildRenderList(RenderList renderList)
        {
            Game.Gui.AddRenderItem(renderList, this);
        }
    }
}
