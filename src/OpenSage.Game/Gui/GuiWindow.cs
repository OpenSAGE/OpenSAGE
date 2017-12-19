using OpenSage.Data.Wnd;
using OpenSage.Gui.Elements;

namespace OpenSage.Gui
{
    public sealed class GuiWindow
    {
        public UIElement Root { get; }

        public GuiWindowCallback LayoutInit { get; }
        public GuiWindowCallback LayoutUpdate { get; }
        public GuiWindowCallback LayoutShutdown { get; }

        internal GuiWindow(WndFile wndFile, UIElement root)
        {
            Root = root;

            LayoutInit = CallbackUtility.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutInit);
            LayoutUpdate = CallbackUtility.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutUpdate);
            LayoutShutdown = CallbackUtility.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutShutdown);
        }
    }

    public delegate void GuiWindowCallback(GuiWindow window);
}
