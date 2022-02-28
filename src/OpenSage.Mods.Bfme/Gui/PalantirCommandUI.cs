using OpenSage.Gui.Apt;
using OpenAS2.Runtime;

namespace OpenSage.Mods.Bfme2.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    class PalantirCommandUI
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void OnSubMenuLoaded(string param, ExecutionContext context, AptWindow window, Game game)
        {
        }

        public static void OnButtonFrameLoaded(string param, ExecutionContext context, AptWindow window, Game game)
        {
        }

        public static void OnToggleFlashLoaded(string param, ExecutionContext context, AptWindow window, Game game)
        {
        }
    }
}
