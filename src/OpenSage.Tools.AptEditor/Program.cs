
using ImGuiNET;
using System;
using System.Numerics;
using OpenSage.Tools.AptEditor.UI;
using OpenSage.Tools.AptEditor.Util;
using Veldrid;
using Veldrid.StartupUtilities;

namespace OpenSage.Tools.AptEditor
{
    class Program
    {
        static string Launcher()
        {
            const int initialWidth = 640;
            const int initialHeight = 360;
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(100, 100, initialWidth, initialHeight, WindowState.Normal, "OpenSAGE Apt Editor"),
                out var window,
                out var graphicsDevice);

            graphicsDevice.SyncToVerticalBlank = true;

            var resultPath = (string)null;

            using (var commandList = graphicsDevice.ResourceFactory.CreateCommandList())
            using (var imGuiRenderer = new ImGuiRenderer(graphicsDevice, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, initialWidth, initialHeight))
            using (var gameTimer = new DeltaTimer())
            {
                window.Resized += () =>
                {
                    graphicsDevice.ResizeMainWindow((uint) window.Width, (uint) window.Height);
                    imGuiRenderer.WindowResized(window.Width, window.Height);
                };

                var windowOpen = true;
                window.Closed += () => windowOpen = false;

                gameTimer.Start();
                byte[] rootPathBuffer = new byte[1024];
                System.Text.Encoding.UTF8.GetBytes("C:\\Users\\lanyi\\Desktop\\RA3Mods\\ARSDK2\\Mods\\test\\Data\\AptUI\\").CopyTo(rootPathBuffer, 0);
                while (windowOpen)
                {
                    commandList.Begin();

                    gameTimer.Update();

                    var inputSnapshot = window.PumpEvents();

                    commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);

                    commandList.ClearColorTarget(0, RgbaFloat.Clear);

                    imGuiRenderer.Update((float)gameTimer.CurrentGameTime.DeltaTime.TotalSeconds, inputSnapshot);

                    ImGui.SetNextWindowPos(new Vector2(0, 0));
                    ImGui.SetNextWindowSize(new Vector2(initialWidth, initialHeight));
                    var open = true;
                    if(ImGui.Begin("Launcher", ref open, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize))
                    {
                        ImGui.Text("Start OpenSage Apt Editor by providing a root path.");
                        ImGuiUtility.InputText("", rootPathBuffer, out var inputRootPath);
                        if(ImGui.Button("Load"))
                        {
                            resultPath = inputRootPath;
                            window.Close();
                        }
                    }
                    ImGui.End();

                    imGuiRenderer.Render(graphicsDevice, commandList);

                    commandList.End();

                    graphicsDevice.SubmitCommands(commandList);

                    graphicsDevice.SwapBuffers();
                }
            }

            graphicsDevice.Dispose();
            return resultPath;
        }

        static void GameWindowAdapter(string rootPath)
        {
            using (var system = new AptGameSystem(rootPath))
            using (var imGuiRenderer = new ImGuiRenderer(system.Game.GraphicsDevice, 
                                                         system.Game.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                                                         system.Game.Window.ClientBounds.Width, system.Game.Window.ClientBounds.Height))
            using (var gameCommandList = system.Game.Window.GraphicsDevice.ResourceFactory.CreateCommandList())
            using (var mainForm = new MainForm(system))
            {
                void OnClientSizeChanged(object sender, EventArgs args)
                {
                    imGuiRenderer.WindowResized(system.Game.Window.ClientBounds.Width, system.Game.Window.ClientBounds.Height);
                }
                system.Game.Window.ClientSizeChanged += OnClientSizeChanged;

                var tickActions = new Action<Game, CommandList>[]{
                    (game, commandList) => {
                        imGuiRenderer.Update((float)game.RenderTime.DeltaTime.TotalSeconds, game.Window.CurrentInputSnapshot);
                        mainForm.Draw(commandList);
                        imGuiRenderer.Render(game.GraphicsDevice, commandList);
                    }
                };

                while(system.CustomGameTick(gameCommandList, tickActions));

                system.Game.Window.ClientSizeChanged -= OnClientSizeChanged;
            }
        }

        static void Main(string[] args)
        {
            Platform.Start();

            var rootPath = Launcher();
            if(rootPath != null)
            {
                GameWindowAdapter(rootPath);
            }

            Platform.Stop();
        }
    }
}
