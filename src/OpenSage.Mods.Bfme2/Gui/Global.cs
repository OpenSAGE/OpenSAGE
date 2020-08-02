using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme2.Gui
{
    [AptCallbacks(SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class Global
    {
        public static void OnAptInGameSpellBookLoaded(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void OnAptInGameSideCommandBarLoaded(string param, ActionContext context, AptWindow window, Game game)
        {

        }

        public static void OnAptInGameSideCommandBarButtonFrameLoaded(string param, ActionContext context, AptWindow window, Game game)
        {

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
