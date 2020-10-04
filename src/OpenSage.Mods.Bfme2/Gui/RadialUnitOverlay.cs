using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.UnitOverlay;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Bfme2.Gui
{
    public class RadialUnitOverlay : InputMessageHandler, IUnitOverlay
    {
        Game _game;

        private bool _visible;
        private Point2D _center;
        private CommandSet _commandSet;
        List<RadialButton> _buttons;
        private GameObject _selectedUnit;

        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public RadialUnitOverlay(Game game)
        {
            _game = game;
            _game.InputMessageBuffer.Handlers.Add(this);
            _buttons = new List<RadialButton>();
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

            var screenPosition = _game.Scene3D.Camera.WorldToScreenPoint(selectedUnit.Collider.WorldBounds.Center);
            _center = new Point2D((int)screenPosition.X, (int)screenPosition.Y);
            _commandSet = selectedUnit.Definition.CommandSet.Value;

            if (_selectedUnit != selectedUnit && _commandSet != null)
            {
                //Update button list
                _buttons.Clear();

                var commandButtons = _commandSet.Buttons.Values.Where(x => x.Value.Radial && x.Value.Command != CommandType.Revive);

                foreach(var commandButton in commandButtons)
                {
                    var radialButton = new RadialButton(_game, selectedUnit, commandButton.Value);
                    _buttons.Add(radialButton);
                }

                _selectedUnit = selectedUnit;
            }

            var isProducing = selectedUnit.ProductionUpdate?.IsProducing ?? false;
            foreach (var radialButton in _buttons)
            {
                var (count, progress) = isProducing ? selectedUnit.ProductionUpdate.GetCountAndProgress(radialButton.CommandButton) : (0, 0.0f);
                radialButton.Update(progress, count, selectedUnit.CanEnqueue(radialButton.CommandButton));
            }
        }

        public void Render(DrawingContext2D drawingContext)
        {
            if (!_visible)
            {
                return;
            }

            var radialBorder = _game.GetMappedImage("RadialBorder");

            // TODO: fill revive buttons with died heroes
            var commandButtons = _commandSet.Buttons.Values.Where(x => x.Value.Radial && x.Value.Command != CommandType.Revive);

            var radius = (-1 + MathF.Sqrt(commandButtons.Count())) * (radialBorder.Coords.Width * 1.1f);
            var deltaAngle = MathUtility.TwoPi / commandButtons.Count();
            var width = radialBorder.Coords.Width;
            var height = radialBorder.Coords.Height;

            var i = 0;
            foreach (var button in _buttons)
            {
                var posX = _center.X + MathF.Sin(i * deltaAngle) * radius;
                var posY = _center.Y + -MathF.Cos(i * deltaAngle) * radius;

                button.Render(drawingContext, new Vector2(posX, posY));
                i++;
            }
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            foreach(var button in _buttons)
            {
                if (button.HandleMouseCursor(message))
                {
                    return InputMessageResult.Handled;
                }
            }
            return InputMessageResult.NotHandled;
        }
    }

    public class RadialUnitOverlaySource : IUnitOverlaySource
    {
        public IUnitOverlay Create(Game game) => new RadialUnitOverlay(game);
    }
}
