using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class AptPalantir
    {
        public static bool Initialized { get; private set; } = false;
        public static int SideButtonsInitialized { get; internal set; } = 0;

        public static void Reset()
        {
            Initialized = false;
            SideButtonsInitialized = 0;
        }

        // Called after the initialization has been performed
        public static void OnInitialized(string param, ActionContext context, AptWindow window, Game game)
        {
            Initialized = true;
        }

        public static void OnBttnOptions(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void OnHeroSelectLoaded(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void OnHelpBoxLoaded(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void OnPlanningModeUILoaded(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void OnSpellBookUIShown(string param, ActionContext context, AptWindow window, Game game)
        {

        }
    }
}
