using System;

namespace OpenSage.Gui.Wnd
{
    internal static class CallbackUtility
    {
        public static WndWindowCallback GetGuiWindowCallback(string name)
        {
            return GetCallback<WndWindowCallback>(name);
        }

        public static UIElementCallback GetUIElementCallback(string name)
        {
            return GetCallback<UIElementCallback>(name);
        }

        public static Action<WndWindow, Game> GetDrawCallback(string name)
        {
            return GetCallback<Action<WndWindow, Game>>(name);
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
