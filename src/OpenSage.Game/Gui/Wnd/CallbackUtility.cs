using System;
using OpenSage.Gui.Wnd.Elements;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Gui.Wnd
{
    internal static class CallbackUtility
    {
        public static GuiWindowCallback GetGuiWindowCallback(string name)
        {
            return GetCallback<GuiWindowCallback>(name);
        }

        public static UIElementCallback GetUIElementCallback(string name)
        {
            return GetCallback<UIElementCallback>(name);
        }

        public static Action<UIElement, GraphicsDevice> GetDrawCallback(string name)
        {
            return GetCallback<Action<UIElement, GraphicsDevice>>(name);
        }

        private static TDelegate GetCallback<TDelegate>(string name)
            where TDelegate : class
        {
            if (string.Equals(name, "[None]", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var callbackMethod = typeof(WndCallbacks).GetMethod(name);
            if (callbackMethod == null) // TODO: Should never be null, but will be during development.
            {
                return null;
            }

            return (TDelegate) (object) Delegate.CreateDelegate(typeof(TDelegate), callbackMethod);
        }
    }
}
