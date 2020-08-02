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

        public AptControlBar(Game game)
        {
            _game = game;
        }

        public void AddToScene(Scene2D scene2D)
        {
            _window = _game.LoadAptWindow("Palantir.apt");

            _game.Scene2D.AptWindowManager.PushWindow(_window);
        }

        static bool shown = false;

        public void Update(Player player)
        {
            if (AptPalantir.Initialized)
            {
                if (player.SelectedUnits.Count >= 0 && !shown)
                {
                    var commandBar = _window.Root.GetNamedItem("SideCommandBar");
                    var func = commandBar.ScriptObject.GetMember("FadeIn");

                    if (func.Type != ValueType.Undefined)
                    {
                        List<Value> emptyArgs = new List<Value>();
                        FunctionCommon.ExecuteFunction(func, emptyArgs.ToArray(), commandBar.ScriptObject, _window.Context.Avm);
                        shown = true;
                    }
                }
                else
                {

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
