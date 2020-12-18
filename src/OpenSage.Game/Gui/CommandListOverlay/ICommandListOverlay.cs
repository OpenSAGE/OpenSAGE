using OpenSage.Logic;

namespace OpenSage.Gui.CommandListOverlay
{
    public interface ICommandListOverlay
    {
        public void Update(Player player);
        public void Render(DrawingContext2D drawingContext);
    }

    public interface ICommandListOverlaySource
    {
        ICommandListOverlay Create(Game game);
    }
}
