using System;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd
{
    public class WndCallbackResolver
    {
        private readonly Type _type;

        internal WndCallbackResolver(Type wndCallbacksType)
        {
            _type = wndCallbacksType;
        }

        internal WindowCallback GetWindowCallback(string name)
        {
            return GetCallback<WindowCallback>(name);
        }

        internal ControlCallback GetControlCallback(string name)
        {
            return GetCallback<ControlCallback>(name);
        }

        internal ControlDrawCallback GetControlDrawCallback(string name)
        {
            return GetCallback<ControlDrawCallback>(name);
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
