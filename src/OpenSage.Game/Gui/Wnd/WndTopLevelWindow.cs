using System.Linq;
using System.Numerics;
using OpenSage.Data.Wnd;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndTopLevelWindow
    {
        public WndWindow Root { get; }

        public WndWindowCallback LayoutInit { get; }
        public WndWindowCallback LayoutUpdate { get; }
        public WndWindowCallback LayoutShutdown { get; }

        internal WndTopLevelWindow(WndFile wndFile, WndWindow root, WndCallbackResolver callbackResolver)
        {
            Root = root;

            LayoutInit = callbackResolver.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutInit);
            LayoutUpdate = callbackResolver.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutUpdate);
            LayoutShutdown = callbackResolver.GetGuiWindowCallback(wndFile.LayoutBlock.LayoutShutdown);
        }

        public WndWindow FindWindow(Vector2 mousePosition)
        {
            // Finds deepest element that is visible and contains mousePosition.
            WndWindow findElementRecursive(WndWindow element)
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
