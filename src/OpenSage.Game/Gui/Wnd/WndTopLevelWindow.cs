using System.Linq;
using System.Numerics;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Elements;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndTopLevelWindow
    {
        public UIElement Root { get; }

        public WndWindowCallback LayoutInit { get; }
        public WndWindowCallback LayoutUpdate { get; }
        public WndWindowCallback LayoutShutdown { get; }

        internal WndTopLevelWindow(WndFile wndFile, UIElement root)
        {
            Root = root;

            LayoutInit = CallbackUtility.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutInit);
            LayoutUpdate = CallbackUtility.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutUpdate);
            LayoutShutdown = CallbackUtility.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutShutdown);
        }

        public UIElement FindElement(Vector2 mousePosition)
        {
            // Finds deepest element that is visible and contains mousePosition.
            UIElement findElementRecursive(UIElement element)
            {
                if (!element.Visible || !element.Frame.Contains(mousePosition))
                {
                    return null;
                }

                foreach (var child in element.Children.Reverse())
                {
                    var found = findElementRecursive(child);
                    if (found != null)
                    {
                        return found;
                    }
                }

                return element;
            }

            return findElementRecursive(Root);
        }
    }

    public delegate void WndWindowCallback(WndTopLevelWindow window);
}
