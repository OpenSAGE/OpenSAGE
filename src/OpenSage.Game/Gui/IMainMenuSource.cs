using OpenSage.Content;

namespace OpenSage.Gui
{
    public interface IMainMenuSource
    {
        void AddToScene(Game game, Scene2D scene, bool useShellMap);
    }
}
