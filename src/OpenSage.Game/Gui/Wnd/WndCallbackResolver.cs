using System;

namespace OpenSage.Gui.Wnd
{
    public class WndCallbackResolver
    {
        private readonly Type _type;

        internal WndCallbackResolver(Type wndCallbacksType)
        {
            _type = wndCallbacksType;
        }

        internal WndWindowCallback GetGuiWindowCallback(string name)
        {
            return GetCallback<WndWindowCallback>(name);
        }

        internal UIElementCallback GetUIElementCallback(string name)
        {
            return GetCallback<UIElementCallback>(name);
        }

        internal UIElementDrawCallback GetDrawCallback(string name)
        {
            return GetCallback<UIElementDrawCallback>(name);
        }

        private TDelegate GetCallback<TDelegate>(string name)
            where TDelegate : class
        {
            if (string.Equals(name, "[None]", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var callbackMethod = _type.GetMethod(name);
            if (callbackMethod == null) // TODO: Should never be null, but will be during development.
            {
                return null;
            }

            return (TDelegate) (object) Delegate.CreateDelegate(typeof(TDelegate), callbackMethod);
        }
    }
}
