using OpenSage.Logic;

namespace OpenSage.Gui.UnitOverlay
{
    public interface IUnitOverlay
    {
        public void Update(Player player);
        public void Render(DrawingContext2D drawingContext);
    }

    public interface IUnitOverlaySource
    {
        IUnitOverlay Create(Game game);
    }
}
