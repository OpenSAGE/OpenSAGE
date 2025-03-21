﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd;

public class WndCallbackResolver
{
    private readonly Dictionary<string, MethodInfo> _callbackCache;

    internal WndCallbackResolver()
    {
        _callbackCache = new Dictionary<string, MethodInfo>();

        // TODO: Filter by mod, perhaps using parameter to [WndCallbacks]?
        // At the moment callbacks from all mods are lumped together.
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.FullName.StartsWith("OpenSage"))
                continue;

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(WndCallbacksAttribute), false).Length > 0)
                {
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => !m.IsSpecialName))
                    {
                        _callbackCache.Add(method.Name, method);
                    }
                }
            }
        }
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

    private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private TDelegate GetCallback<TDelegate>(string name)
        where TDelegate : class
    {
        if (string.Equals(name, "[None]", StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        if (!_callbackCache.TryGetValue(name, out var method))
        {
            Logger.Warn($"Failed to resolve callback '{name}'");
            return null;
        }

        return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), method);
    }
}
