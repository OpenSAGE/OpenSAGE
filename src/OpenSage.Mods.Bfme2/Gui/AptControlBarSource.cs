using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Mods.Bfme2.Gui;
using Veldrid;
using Geometry = OpenSage.Data.Apt.Geometry;
using Rectangle = OpenSage.Mathematics.Rectangle;
using ValueType = OpenSage.Gui.Apt.ActionScript.ValueType;

namespace OpenSage.Mods.Bfme2
{
    class AptControlBar : IControlBar
    {
        Game _game;
        AptWindow _window;
        SpriteItem _root;
        private bool _commandbarVisible = false;
        private bool _palantirInitialized = false;
        private bool _minimapInitialized = false;
        private GameObject _selectedUnit;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public AptControlBar(Game game)
        {
            _game = game;
        }

        public void AddToScene(Scene2D scene2D)
        {
            AptPalantir.Reset();

            _window = _game.LoadAptWindow("Palantir.apt");
            _root = _window.Root;

            _game.Scene2D.AptWindowManager.PushWindow(_window);
        }

        private void SetMinimap()
        {
            if (!_minimapInitialized)
            {
                var radar = _root.ScriptObject.GetMember("Radar").ToObject();
                var radarClipValue = radar.GetMember("RadarClip");

                if (radarClipValue.Type == ValueType.Undefined)
                {
                    return;
                }

                // This shape is used to render the minimap
                var radarClip = radarClipValue.ToObject().Item as SpriteItem;
                var shape = radarClip.Content.Items[1] as RenderItem;
                shape.RenderCallback = (AptRenderingContext renderContext, Geometry geom, Texture orig) =>
                {
                    var rect = new Rectangle(renderContext.GetBoundingBox(geom));
                    _game.Scene3D.Radar.DrawRadarMinimap(renderContext.GetActiveDrawingContext(), rect, true);
                };

                // This shape is used to render the overlay
                var radarChild = ((SpriteItem) radar.Item).Content.Items[1] as SpriteItem;
                shape = radarChild.Content.Items[1] as RenderItem;
                shape.RenderCallback = (AptRenderingContext renderContext, Geometry geom, Texture orig) =>
                {
                    var rect = new Rectangle(renderContext.GetBoundingBox(geom));
                    _game.Scene3D.Radar.DrawRadarOverlay(renderContext.GetActiveDrawingContext(), rect);
                };

                _minimapInitialized = true;
            }
        }

        private void InitializePalantir(Player player)
        {
            if (!_palantirInitialized)
            {
                logger.Info("Initialize Palantir!");

                var showCommandInterface = _root.ScriptObject.GetMember("SetPalantirFrameState");
                if (showCommandInterface.Type != ValueType.Undefined)
                {
                    bool good = Array.Exists(player.Template.IntrinsicSciences, s => s == "SCIENCE_GOOD");
                    List<Value> emptyArgs = new List<Value>();
                    emptyArgs.Add(Value.FromString(good ? "_good" : "_evil"));
                    FunctionCommon.ExecuteFunction(showCommandInterface, emptyArgs.ToArray(), _root.ScriptObject, _window.Context.Avm);
                    _palantirInitialized = true;
                }
            }
        }

        private void UpdateCommandbuttons()
        {
            var commandButtons = _root.ScriptObject.GetMember("CommandButtons").ToObject();

            for (int i = 0; i < 6; i++)
            {
                var commandButton = commandButtons.GetMember(i.ToString()).ToObject();
                var createContent = commandButton.GetMember("CreateContent");
                List<Value> args = new List<Value>();
                args.Add(Value.FromString("bttn"));
                args.Add(Value.FromString("CommandButton"));

                //TODO: fix so this works
                FunctionCommon.ExecuteFunction(createContent, args.ToArray(), commandButton.Item.ScriptObject, _window.Context.Avm);
            }
        }


        private void ApplyPortrait(GameObject unit, ObjectContext frame)
        {
            if (frame == null || ((SpriteItem) frame.Item).Content.Items.Count == 0)
            {
                return;
            }

            var shape = ((SpriteItem) frame.Item).Content.Items[1] as RenderItem;

            if (shape.RenderCallback != null && _selectedUnit == unit)
            {
                return;
            }

            if (unit != null && unit.Definition.SelectPortrait != null)
            {
                //var shape = frameSprite.Content.Items[1] as RenderItem;
                shape.RenderCallback = (AptRenderingContext renderContext, Geometry geom, Texture orig) =>
                {
                    renderContext.RenderGeometry(geom, orig);

                    var destRect = RectangleF.Transform(geom.BoundingBox, renderContext.GetCurrentTransformMatrix());
                    var selectPortrait = unit.Definition.SelectPortrait.Value;
                    var sourceRect = selectPortrait.Coords;

                    renderContext.GetActiveDrawingContext().DrawImage(selectPortrait.Texture.Value, sourceRect, destRect);
                };
            }
            else
            {
                shape.RenderCallback = null;
            }
        }

        private void UpdatePalantir(Player player)
        {
            var palantirFrame = _root.ScriptObject.GetMember("CommandBackground").ToObject();

            if (player.SelectedUnits.Count > 0)
            {
                // TODO: Handle multiple selection.
                var unit = player.SelectedUnits.First();

                // TODO: Only do this when command set changes.
                ApplyPortrait(unit, palantirFrame);

                _selectedUnit = unit;
            }
            else
            {
                ApplyPortrait(null, palantirFrame);
            }
        }

        private void UpdateSideCommandbar(Player player)
        {
            var sideCommandBar = _root.ScriptObject.GetMember("SideCommandBar").ToObject();

            if (player.SelectedUnits.Count > 0 && !_commandbarVisible)
            {
                var fadeIn = sideCommandBar.Item.ScriptObject.GetMember("FadeIn");

                if (fadeIn.Type != ValueType.Undefined)
                {
                    List<Value> emptyArgs = new List<Value>();
                    FunctionCommon.ExecuteFunction(fadeIn, emptyArgs.ToArray(), sideCommandBar.Item.ScriptObject, _window.Context.Avm);
                    _commandbarVisible = true;
                }

                UpdateCommandbuttons();
            }
            else if (player.SelectedUnits.Count == 0 && _commandbarVisible)
            {
                var fadeOut = sideCommandBar.Item.ScriptObject.GetMember("FadeOut");

                if (fadeOut.Type != ValueType.Undefined)
                {
                    List<Value> emptyArgs = new List<Value>();
                    FunctionCommon.ExecuteFunction(fadeOut, emptyArgs.ToArray(), sideCommandBar.Item.ScriptObject, _window.Context.Avm);
                    _commandbarVisible = true;
                }

                _commandbarVisible = false;
            }
        }

        public void Update(Player player)
        {
            if (AptPalantir.Initialized)
            {
                InitializePalantir(player);
                SetMinimap();
                if (AptPalantir.SideButtonsInitialized == 12)
                {
                    UpdateSideCommandbar(player);
                }
                UpdatePalantir(player);
            }
        }
    }

    class AptControlBarSource : IControlBarSource
    {
        public IControlBar Create(string side, Game game)
        {
            return new AptControlBar(game);
        }
    }
}
