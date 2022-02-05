using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static OpenAS2.HostObjects.AptWindow;

namespace OpenAS2.HostObjects
{
    public class AptCallbackResolver
    {
        private readonly Dictionary<string, MethodInfo> _callbackCache;

        internal AptCallbackResolver(Game game)
        {
            _callbackCache = new Dictionary<string, MethodInfo>();

            // TODO: Filter by mod, perhaps using parameter to [AptCallbacks]?
            // At the moment callbacks from all mods are lumped together.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.FullName.StartsWith("OpenSage"))
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(AptCallbacksAttribute), false).Length > 0)
                    {
                        var attribute = type.GetCustomAttribute<AptCallbacksAttribute>();

                        if (!attribute.Games.Contains(game.SageGame))
                            continue;

                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => !m.IsSpecialName))
                        {
                            if (type.Name == "Global")
                            {
                                _callbackCache.Add(method.Name, method);
                            }
                            else
                            {
                                _callbackCache.Add(type.Name + "::" + method.Name, method);
                            }
                        }
                    }
                }
            }
        }

        internal ActionscriptCallback GetCallback(string name)
        {
            return GetCallback<ActionscriptCallback>(name);
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private TDelegate GetCallback<TDelegate>(string name)
            where TDelegate : class
        {

            if (!_callbackCache.TryGetValue(name, out var method))
            {
                logger.Warn($"Failed to resolve callback '{name}'");
                return null;
            }

            return (TDelegate) (object) Delegate.CreateDelegate(typeof(TDelegate), method);
        }
    }
}
