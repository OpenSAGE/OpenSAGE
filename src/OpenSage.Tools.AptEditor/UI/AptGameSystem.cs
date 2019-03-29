using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Apt;
using Veldrid;

namespace OpenSage.Tools.AptEditor.UI
{
    internal class AptGameSystem : DisposableBase
    {
        public Game Game;
        public ContentManager ContentManager => Game.ContentManager;

        public AptGameSystem(string rootPath)
        {
            var installation = new GameInstallation(new AptEditorDefinition(), rootPath);
            Game = AddDisposable(new Game(installation, null));
        }

        public bool CustomGameTick(CommandList commandList, IEnumerable<Action<Game, CommandList>> tickActions)
        {
            if (!Game.Window.PumpEvents())
            {
                return false;
            }

            commandList.Begin();

            commandList.SetFramebuffer(Game.GraphicsDevice.MainSwapchain.Framebuffer);

            commandList.ClearColorTarget(0, RgbaFloat.Clear);

            foreach(var tickAction in tickActions)
            {
                tickAction(Game, commandList);
            }

            commandList.End();

            Game.GraphicsDevice.SubmitCommands(commandList);

            Game.Window.MessageQueue.Clear();

            Game.GraphicsDevice.SwapBuffers();

            return true;
        }
    }
}