using System.Collections.Generic;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic;
using OpenSage.Mods.Bfme2.Gui;

namespace OpenSage.Mods.Bfme2
{
    class AptControlBar : IControlBar
    {
        Game _game;
        AptWindow _window;
        SpriteItem _root;

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

        private bool _commandbarVisible = false;
        private bool _commandInterfaceVisible = false;

        private void InitializePalantir(Player player)
        {
            if (!_commandInterfaceVisible)
            {
                var showCommandInterface = _root.ScriptObject.GetMember("SetPalantirFrameState");
                if (showCommandInterface.Type != ValueType.Undefined)
                {
                    bool good = System.Array.Exists(player.Template.IntrinsicSciences, s => s == "SCIENCE_GOOD");
                    List<Value> emptyArgs = new List<Value>();
                    emptyArgs.Add(Value.FromString(good ? "_good" : "_evil"));
                    FunctionCommon.ExecuteFunction(showCommandInterface, emptyArgs.ToArray(), _root.ScriptObject, _window.Context.Avm);
                    _commandInterfaceVisible = true;
                }
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
                if (AptPalantir.SideButtonsInitialized == 12)
                {
                    UpdateSideCommandbar(player);
                }
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
