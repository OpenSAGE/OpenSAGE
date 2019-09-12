using OpenSage.Content;

namespace OpenSage.Gui.Wnd
{
    public class WndMainMenuSource : IMainMenuSource
    {
        private readonly string _wndFileName;

        public WndMainMenuSource(string wndFileName)
        {
            _wndFileName = wndFileName;
        }

        public void AddToScene(Game game, Scene2D scene, bool useShellMap)
        {
            scene.WndWindowManager.PushWindow(_wndFileName);
        }
    }
}
