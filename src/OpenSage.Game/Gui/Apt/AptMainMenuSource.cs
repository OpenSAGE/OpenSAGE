using OpenSage.Content;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public class AptMainMenuSource : IMainMenuSource
    {
        private readonly string _aptFileName;
        private readonly string _fallbackShell;

        public AptMainMenuSource(string aptFileName, string fallbackShell)
        {
            _aptFileName = aptFileName;
            _fallbackShell = fallbackShell;
        }

        public void AddToScene(ContentManager contentManager, Scene2D scene)
        {
            var aptWindow = contentManager.Load<AptWindow>(_aptFileName);
            aptWindow.BackgroundImage = contentManager.Load<Texture>(_fallbackShell);
            scene.AptWindowManager.PushWindow(aptWindow);
        }
    }
}
