using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;

namespace OpenSage.DataViewer.Framework
{
    // TODO: Some of this could be moved to the engine.
    internal static class InstallationUtility
    {
        public static IEnumerable<GameInstallation> FindInstallations()
        {
            IInstallationLocator locator;

            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                locator = new RegistryInstallationLocator();
            }
            else
            {
                locator = new EnvironmentInstallationLocator();
            }
            
            return GameDefinition.All.SelectMany(locator.FindInstallations);
        }
    }
}
