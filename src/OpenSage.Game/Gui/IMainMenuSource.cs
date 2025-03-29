using OpenSage.Content;

namespace OpenSage.Gui;

public interface IMainMenuSource
{
    void AddToScene(IGame game, Scene2D scene, bool useShellMap);
}
