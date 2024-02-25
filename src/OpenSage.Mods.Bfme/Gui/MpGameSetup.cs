using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme2.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class MpGameSetup
    {
        // Called after the initialization has been performed
        public static void OnReadyPress(string param, ActionContext context, AptWindow window, Game game)
        {
        }
    }
}
