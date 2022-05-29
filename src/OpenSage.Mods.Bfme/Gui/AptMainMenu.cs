using OpenSage.FileFormats.Apt;
using OpenSage.Gui.Apt;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Library;
using Veldrid;

namespace OpenSage.Mods.Bfme.Gui
{
    [AptCallbacks(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk)]
    static class AptMainMenu
    {
        // Called after the initialization has been performed
        public static void OnInitialized(string param, ExecutionContext context, AptWindow window, Game game)
        {
            // Set a custom render callback
            var logoTexture = game.GetMappedImage("LogoWithShadow").Texture.Value;
            var imageSprite = window.Root.ScriptObject.GetMember("Image").ToObject<StageObject>().Item as SpriteItem;
            var shape = imageSprite.Content.Items[1] as RenderItem;
            shape.RenderCallback = (AptRenderingContext renderContext, Geometry geom, Texture orig) =>
            {
                renderContext.RenderGeometry(geom, logoTexture);
            };
        }

        // Close the game
        public static void ExitGame(string param, ExecutionContext context, AptWindow window, Game game)
        {
            game.Exit();
        }

        public static void StopGameMovie(string param, ExecutionContext context, AptWindow window, Game game)
        {

        }

        public static void ResetResolution(string param, ExecutionContext context, AptWindow window, Game game)
        {
            // Probably used for debug purposes
        }

        public static void CreateAHero(string param, ExecutionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("CreateAHero.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void Options(string param, ExecutionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("Options.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void Skirmish(string param, ExecutionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("Skirmish.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void LAN(string param, ExecutionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("LanOpenPlay.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void Credits(string param, ExecutionContext context, AptWindow window, Game game)
        {
        }

        public static void OnTutorial(string param, ExecutionContext context, AptWindow window, Game game)
        {
        }

        public static void MultiplayerButtonPressed(string param, ExecutionContext context, AptWindow window, Game game)
        {
            var aptWindow = game.LoadAptWindow("MPGameSetup.apt");
            game.Scene2D.AptWindowManager.QueryTransition(aptWindow);
        }

        public static void BattleSchool(string param, ExecutionContext context, AptWindow window, Game game)
        {

        }
    }
}
