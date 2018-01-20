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
            var locator = new RegistryInstallationLocator();
            return GameDefinition.All.SelectMany(locator.FindInstallations);
        }
    }
}
