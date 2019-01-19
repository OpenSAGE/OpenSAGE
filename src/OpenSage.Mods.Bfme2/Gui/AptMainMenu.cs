using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Mods.Bfme2.Gui
{
    [AptCallbacks(SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class AptMainMenu
    {
        public static void OnInitialized(ActionContext context, AptWindow window, Game game)
        {
            // Set the logo texture
            var imageSprite = window.Root.ScriptObject.Variables["Image"].ToObject().Item as SpriteItem;
            var shape = imageSprite.Content.Items[1] as RenderItem;
            shape.Texture = window.ImageLoader.GetMappedImage("LogoWithShadow").Texture;
        }
    }
}
