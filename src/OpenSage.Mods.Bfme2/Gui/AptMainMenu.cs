using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme2.Gui
{
    [AptCallbacks(SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class AptMainMenu
    {
        // Called after the initialization has been performed
        public static void OnInitialized(string param, ActionContext context, AptWindow window, Game game)
        {
            // Set the logo texture
            var imageSprite = window.Root.ScriptObject.Variables["Image"].ToObject().Item as SpriteItem;
            var shape = imageSprite.Content.GetItems()[1] as RenderItem;
            shape.Texture = window.ImageLoader.GetMappedImage("LogoWithShadow").Texture;
        }

        // Close the game
        public static void ExitGame(string param, ActionContext context, AptWindow window, Game game)
        {
            game.Window.Close();
        }

        public static void ResetResolution(string param, ActionContext context, AptWindow window, Game game)
        {
            // Probably used for debug purposes
        }

        public static void CreateAHero(string param, ActionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.ContentManager.Load<AptWindow>("Skirmish.apt");

            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }
    }
}
