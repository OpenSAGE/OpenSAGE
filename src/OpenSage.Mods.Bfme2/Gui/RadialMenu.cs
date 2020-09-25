using System.Linq;
using OpenSage.Gui;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Bfme2.Gui
{
    public class RadialMenu
    {
        Game _game;

        private Point2D _center;

        public RadialMenu(Game game)
        {
            _game = game;
        }

        public void Update(Player player)
        {
            if (player.SelectedUnits.Count != 1)
            {
                return;
            }

            var selectedUnit = player.SelectedUnits.First();
            if (selectedUnit.Definition.KindOf.Get(ObjectKinds.Structure))
            {
                return;
            }

            var screenSpaceBoundingRectangle = selectedUnit.Collider.GetBoundingRectangle(_game.Scene3D.Camera);
            _center = screenSpaceBoundingRectangle.Center;
        }

        public void Render(DrawingContext2D drawingContext)
        {
            
        }
    }
}
