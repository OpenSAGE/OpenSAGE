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

        public void AddToScene(ContentManager contentManager, Scene2D scene, bool useShellMap)
        {
            var aptWindow = contentManager.Load<AptWindow>(_aptFileName);
            if (!useShellMap)
            {
                aptWindow.BackgroundImage = contentManager.GetMappedImage(_fallbackShell);
            }
            scene.AptWindowManager.PushWindow(aptWindow);
        }
    }
}
