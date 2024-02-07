using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt;

namespace OpenSage.Mods.Bfme.Gui
{
    [AptCallbacks(SageGame.Bfme)]
    public class AptSaveLoad
    {
        public static void OnInitialized(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void Initialized(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void LoadMenu(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void StringName(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void SelectCampaign(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void DisableComponents(string param, ActionContext context, AptWindow window, Game game)
        {
            // do we need to hide the buttons from MainMenu here?
        }

        public static void Exit(string param, ActionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("MainMenu.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }
    }
}
