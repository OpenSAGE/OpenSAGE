using OpenSage.Content;

namespace OpenSage.Gui
{
    public interface IMainMenuSource
    {
        void AddToScene(ContentManager contentManager, Scene2D scene, bool useShellMap);
    }
}
