using OpenSage.Content;

namespace OpenSage.Gui.Apt
{
    public class AptMainMenuSource : IMainMenuSource
    {
        private readonly string _aptFileName;
        private readonly string _fallbackShell;

        public AptMainMenuSource(string aptFileName)
        {
            _aptFileName = aptFileName;
            _fallbackShell = "ShellMapLowLOD";
        }

        public void AddToScene(Game game, Scene2D scene, bool useShellMap)
        {
            var aptWindow = game.LoadAptWindow(_aptFileName);
            if (!useShellMap)
            {
                aptWindow.BackgroundImage = game.AssetStore.MappedImages.GetByKey(_fallbackShell);
            }
            scene.AptWindowManager.PushWindow(aptWindow);
        }
    }
}
