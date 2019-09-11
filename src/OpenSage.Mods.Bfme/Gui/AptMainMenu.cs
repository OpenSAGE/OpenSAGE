using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme.Gui
{
    [AptCallbacks(SageGame.Bfme)]
    static class AptMainMenu
    {
        // Called after the initialization has been performed
        public static void OnInitialized(string param, ActionContext context, AptWindow window, Game game)
        {
            // Set the logo texture
            var imageSprite = window.Root.ScriptObject.Variables["Image"].ToObject().Item as SpriteItem;
            var shape = imageSprite.Content.Items[1] as RenderItem;
            shape.Texture = game.ContentManager.GetMappedImage("LogoWithShadow").Texture.Value;
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
    }
}
