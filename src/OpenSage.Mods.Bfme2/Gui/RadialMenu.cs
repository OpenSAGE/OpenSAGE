using System;
using System.Linq;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Bfme2.Gui
{
    public class RadialMenu
    {
        Game _game;

        private bool _visible;
        private Point2D _center;
        private CommandSet _commandSet;

        public RadialMenu(Game game)
        {
            _game = game;
        }

        public void Update(Player player)
        {
            _visible = false;
            if (player.SelectedUnits.Count != 1)
            {
                return;
            }

            var selectedUnit = player.SelectedUnits.First();
            if (!selectedUnit.Definition.KindOf.Get(ObjectKinds.Structure))
            {
                return;
            }

            if (selectedUnit.Definition.CommandSet == null)
            {
                return;
            }

            _visible = true;

            var screenSpaceBoundingRectangle = selectedUnit.Collider.GetBoundingRectangle(_game.Scene3D.Camera);
            _center = screenSpaceBoundingRectangle.Center;
            _commandSet = selectedUnit.Definition.CommandSet.Value;
        }

        public void Render(DrawingContext2D drawingContext)
        {
            if (!_visible)
            {
                return;
            }

            var radialBorder = _game.GetMappedImage("RadialBorder");
            var i = 0;

            // TODO: fill revive buttons with died heros
            var commandButtons = _commandSet.Buttons.Values.Where(x => x.Value.Radial && x.Value.Command != CommandType.Revive);

            var radius = (-1 + MathF.Sqrt(commandButtons.Count())) * (radialBorder.Coords.Width);
            var deltaAngle = MathUtility.TwoPi / commandButtons.Count();
            var width = radialBorder.Coords.Width;
            var height = radialBorder.Coords.Height;

            foreach (var button in commandButtons)
            {
                var posX = _center.X + MathF.Sin(i * deltaAngle) * radius;
                var posY = _center.Y + -MathF.Cos(i * deltaAngle) * radius;

                var rect = new RectangleF(posX - width / 2, posY - width / 2, width, height);
                drawingContext.DrawMappedImage(button.Value.ButtonImage.Value, rect.Scale(0.9f));
                drawingContext.DrawMappedImage(radialBorder, rect);
                i++;
            }
        }
    }
}
