using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class Global
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void OnAptInGameSpellBookLoaded(string param, ActionContext context, AptWindow window, Game game)
        {
            logger.Info("InGame spellbook loaded!");
        }

        public static void OnAptInGameSideCommandBarLoaded(string param, ActionContext context, AptWindow window, Game game)
        {
            logger.Info("InGame commandbar loaded!");
        }

        public static void OnAptInGameSideCommandBarButtonFrameLoaded(string param, ActionContext context, AptWindow window, Game game)
        {
            AptPalantir.SideButtonsInitialized++;
        }

        public static void OnAptInGameSideCommandBarFadeOutComplete(string param, ActionContext context, AptWindow window, Game game)
        {
            logger.Info($"InGame sidecommandbar fadeout");
        }

        public static void OnAptInGameSideCommandBarFadeInComplete(string param, ActionContext context, AptWindow window, Game game)
        {
            logger.Info("InGame commandbar fadein complete!");
        }

        public static void SetBackground(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void PlaySound(string param, ActionContext context, AptWindow window, Game game)
        {
            game.Audio.PlayAudioEvent(param);
        }
    }
}
