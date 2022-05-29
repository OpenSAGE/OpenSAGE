using OpenSage.Gui.Apt;
using OpenAS2.Runtime;

namespace OpenSage.Mods.Bfme.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class AptSkirmish
    {
        // Called after the initialization has been performed
        public static void OnInitialized(string param, ExecutionContext context, AptWindow window, Game game)
        {
        }

        public static void Exit(string param, ExecutionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("MainMenu.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void DisableComponents(string param, ExecutionContext context, AptWindow window, Game game)
        {
            // do we need to hide the buttons from MainMenu here?
        }
    }
}
