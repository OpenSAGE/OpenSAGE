using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class AptSkirmish
    {
        // Called after the initialization has been performed
        public static void OnInitialized(string param, ActionContext context, AptWindow window, Game game)
        {
        }

        public static void Exit(string param, ActionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("MainMenu.apt");

            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void DisableComponents(string param, ActionContext context, AptWindow window, Game game)
        {
            //TODO: no idea
        }
    }
}
