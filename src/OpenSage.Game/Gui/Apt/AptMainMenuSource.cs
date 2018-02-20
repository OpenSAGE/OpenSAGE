using OpenSage.Content;

namespace OpenSage.Gui.Apt
{
    public class AptMainMenuSource : IMainMenuSource
    {
        private readonly string _aptFileName;

        public AptMainMenuSource(string aptFileName)
        {
            _aptFileName = aptFileName;
        }

        public void AddToScene(ContentManager contentManager, Scene2D scene)
        {
            var aptWindow = contentManager.Load<AptWindow>(_aptFileName);
            scene.AptWindowManager.PushWindow(aptWindow);
        }
    }
}
