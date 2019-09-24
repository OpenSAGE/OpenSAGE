using System;
using System.Linq;
using OpenSage.Content;

namespace OpenSage.Gui.ControlBar
{
    public static class ControlBarSchemeExtensions
    {
        public static ControlBarScheme FindBySide(this ScopedAssetCollection<ControlBarScheme> assetCollection, string side)
        {
            // Based on a comment in PlayerTemplate.ini about how control bar schemes are chosen.
            return assetCollection.FirstOrDefault(x => x.Side == side)
                ?? assetCollection.FirstOrDefault(x => x.Side == "Observer")
                ?? throw new InvalidOperationException("No ControlBarScheme could be found for the specified side, and no ControlBarScheme for the Observer side could be found either.");
        }
    }
}
