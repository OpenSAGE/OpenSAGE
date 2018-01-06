using System;
using OpenSage.Gui.Wnd.Elements;

namespace OpenSage.Gui.Wnd
{
    internal static class UIElementExtensions
    {
        public static void DoActionRecursive(this UIElement element, Func<UIElement, bool> action)
        {
            if (!action(element))
            {
                return;
            }

            foreach (var child in element.Children)
            {
                child.DoActionRecursive(action);
            }
        }
    }
}
