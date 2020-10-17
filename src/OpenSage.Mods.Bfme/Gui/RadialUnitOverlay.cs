using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.UnitOverlay;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Bfme.Gui
{
    public class RadialUnitOverlay : InputMessageHandler, IUnitOverlay
    {
        Game _game;

        private bool _visible;
        private Point2D _center;
        private CommandSet _commandSet;
        List<RadialButton> _buttons;
        private GameObject _selectedObject;

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
                _selectedObject = null;
                return;
            }

            var selectedObject = player.SelectedUnits.First();
            if (selectedObject.Owner != player
                || selectedObject.IsBeingConstructed()
                || !selectedObject.Definition.KindOf.Get(ObjectKinds.Structure)
                || selectedObject.Definition.CommandSet == null)
            {
                _selectedObject = null;
                return;
            }

            var playerTemplate = selectedObject.Owner.Template;
            _visible = true;

            var screenPosition = _game.Scene3D.Camera.WorldToScreenPoint(selectedObject.Collider.WorldBounds.Center);
            _center = new Point2D((int)screenPosition.X, (int)screenPosition.Y);
            _commandSet = selectedObject.Definition.CommandSet.Value;

            if (_selectedObject != selectedObject && _commandSet != null)
            {
                //Update button list
                var heroIndex = 0;

                var commandButtons = new List<CommandButton>();
                foreach (var button in _commandSet.Buttons.Values)
                {
                    if (button.Value.Command == CommandType.Revive)
                    {
                        if (heroIndex < playerTemplate.BuildableHeroesMP.Count())
                        {
                            var heroDefinition = playerTemplate.BuildableHeroesMP[heroIndex++];
                            commandButtons.Add(new CommandButton(CommandType.UnitBuild, heroDefinition));
                        }
                    }
                    else
                    {
                        commandButtons.Add(button.Value);
                    }
                }

                _buttons.Clear();
                foreach (var commandButton in commandButtons)
                {
                    var isHeroButton = commandButton.Object?.Value?.KindOf.Get(ObjectKinds.Hero) ?? false;
                    var radialButton = new RadialButton(_game, selectedObject, commandButton, isHeroButton);
                    _buttons.Add(radialButton);
                }

                _selectedObject = selectedObject;
            }

            var isProducing = selectedObject.ProductionUpdate?.IsProducing ?? false;
            foreach (var radialButton in _buttons)
            {
                radialButton.IsVisible = true;
                if (radialButton.IsRecruitHeroButton)
                {
                    var definition = radialButton.CommandButton.Object.Value;
                    radialButton.IsVisible = selectedObject.CanRecruitHero(definition);
                }

                var (count, progress) = isProducing ? selectedObject.ProductionUpdate.GetCountAndProgress(radialButton.CommandButton) : (0, 0.0f);
                radialButton.Update(progress, count, selectedObject.CanPurchase(radialButton.CommandButton));
            }
        }

        public void Render(DrawingContext2D drawingContext)
        {
            if (!_visible)
            {
                return;
            }

            var radialBorder = _game.GetMappedImage("RadialBorder");

            var numVisibleButtons = _buttons.Where(x => x.IsVisible).Count();
            var radius = (-1 + MathF.Sqrt(numVisibleButtons + 0.75f)) * (radialBorder.Coords.Width * 0.9f);
            var deltaAngle = MathUtility.TwoPi / numVisibleButtons;

            var i = 0;
            foreach (var button in _buttons)
            {
                if (!button.IsVisible)
                {
                    continue;
                }

                var posX = _center.X + MathF.Sin(i * deltaAngle) * radius;
                var posY = _center.Y + -MathF.Cos(i * deltaAngle) * radius;

                button.Render(drawingContext, new Vector2(posX, posY));
                i++;
            }
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            if (_visible)
            {
                foreach (var button in _buttons)
                {
                    if (button.HandleMouseCursor(message))
                    {
                        return InputMessageResult.Handled;
                    }
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
