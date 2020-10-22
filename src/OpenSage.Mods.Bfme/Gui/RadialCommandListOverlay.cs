using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.CommandListOverlay;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Bfme.Gui
{
    public class RadialCommandListOverlay : InputMessageHandler, ICommandListOverlay
    {
        Game _game;

        private bool _visible;
        private Point2D _center;
        private CommandSet _commandSet;
        List<RadialButton> _buttons;
        private GameObject _selectedUnit;

        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public RadialCommandListOverlay(Game game)
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
                _selectedUnit = null;
                return;
            }

            var selectedStructure = player.SelectedUnits.First();
            if (!selectedStructure.Definition.KindOf.Get(ObjectKinds.Structure))
            {
                return;
            }

            if (selectedStructure.Definition.CommandSet == null)
            {
                _selectedUnit = null;
                return;
            }

            _visible = true;

            var screenPosition = _game.Scene3D.Camera.WorldToScreenPoint(selectedStructure.Collider.WorldBounds.Center);
            _center = new Point2D((int) screenPosition.X, (int) screenPosition.Y);
            _commandSet = selectedStructure.Definition.CommandSet.Value;

            if (_selectedUnit != selectedStructure && _commandSet != null)
            {
                //Update button list
                var playerTeamplate = selectedStructure.Owner.Template;
                var heroIndex = 0;

                var commandButtons = new List<CommandButton>();
                foreach (var button in _commandSet.Buttons.Values)
                {
                    if (button.Value.Command == CommandType.Revive)
                    {
                        if (heroIndex < playerTeamplate.BuildableHeroesMP.Count())
                        {
                            var heroDefinition = playerTeamplate.BuildableHeroesMP[heroIndex++];
                            if (selectedStructure.CanRecruitHero(heroDefinition.Value))
                            {
                                commandButtons.Add(new CommandButton(CommandType.UnitBuild, heroDefinition));
                            }
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
                    var radialButton = new RadialButton(_game, selectedStructure, commandButton);
                    _buttons.Add(radialButton);
                }

                _selectedUnit = selectedStructure;
            }

            var isProducing = selectedStructure.ProductionUpdate?.IsProducing ?? false;
            foreach (var radialButton in _buttons)
            {
                var (count, progress) = isProducing ? selectedStructure.ProductionUpdate.GetCountAndProgress(radialButton.CommandButton) : (0, 0.0f);
                radialButton.Update(progress, count, selectedStructure.CanPurchase(radialButton.CommandButton));
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

    public class RadialUnitOverlaySource : ICommandListOverlaySource
    {
        public ICommandListOverlay Create(Game game) => new RadialCommandListOverlay(game);
    }
}
