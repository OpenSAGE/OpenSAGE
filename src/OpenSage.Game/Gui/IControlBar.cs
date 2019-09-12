using OpenSage.Content;
using OpenSage.Logic;

namespace OpenSage.Gui
{
    public interface IControlBar
    {
        void AddToScene(Scene2D scene2D);
        void Update(Player player);
    }

    public interface IControlBarSource
    {
        IControlBar Create(string side, Game game);
    }
}
