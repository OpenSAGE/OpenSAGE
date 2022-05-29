using OpenSage.Gui.Apt;
using OpenAS2.Runtime;

namespace OpenSage.Mods.Bfme2.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class MpGameSetup
    {
        // Called after the initialization has been performed
        public static void OnReadyPress(string param, ExecutionContext context, AptWindow window, Game game)
        {
        }
    }
}
