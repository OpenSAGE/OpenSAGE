using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme2.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class AptOptions
    {
        // Called after the initialization has been performed
        public static void OnInitialized(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void Cancel(string param, ActionContext context, AptWindow window, Game game)
        {
            // TODO: reset?
            var aptWindow = game.LoadAptWindow("MainMenu.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void Reset(string param, ActionContext context, AptWindow window, Game game)
        {
        }

        public static void Save(string param, ActionContext context, AptWindow window, Game game)
        {
            // TODO: save
            var aptWindow = game.LoadAptWindow("MainMenu.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }
    }
}
