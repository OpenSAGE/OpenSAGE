using System;
using OpenSage.Gui.Wnd.Elements;

namespace OpenSage.Gui.Wnd
{
    internal static class WndWindowExtensions
    {
        public static void DoActionRecursive(this WndWindow element, Func<WndWindow, bool> action)
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
