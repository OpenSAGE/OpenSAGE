
using ImGuiNET;
using System;
using System.Numerics;
using OpenSage.Tools.AptEditor.UI;
using OpenSage.Tools.AptEditor.Util;
using Veldrid;
using Veldrid.StartupUtilities;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Apt.Writer;
using OpenSage.Graphics.Rendering;
using System.Diagnostics;

namespace OpenSage.Tools.AptEditor
{
    class Program
    {
        static string? Launcher()
        {
            const int initialWidth = 640;
            const int initialHeight = 360;
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(100, 100, initialWidth, initialHeight, WindowState.Normal, "OpenSAGE Apt Editor"),
                out var window,
                out var graphicsDevice);

            graphicsDevice.SyncToVerticalBlank = true;

            string? resultPath = null;

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
                System.Text.Encoding.UTF8.GetBytes(@"D:\lanyi\Desktop\RA3Mods\ARSDK2\Mods\Armor Rush\DATA\APTUI\fe_shared_mainMenuLib\").CopyTo(rootPathBuffer, 0);
                while (windowOpen)
                {
                    commandList.Begin();

                    gameTimer.Update();

                    var inputSnapshot = window.PumpEvents();

                    commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);

                    commandList.ClearColorTarget(0, RgbaFloat.Clear);

                    imGuiRenderer.Update((float) gameTimer.CurrentGameTime.DeltaTime.TotalSeconds, inputSnapshot);

                    ImGui.SetNextWindowPos(new Vector2(0, 0));
                    ImGui.SetNextWindowSize(new Vector2(initialWidth, initialHeight));
                    var open = true;
                    if (ImGui.Begin("Launcher", ref open, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize))
                    {
                        ImGui.Text("Start OpenSage Apt Editor by providing a root path.");
                        ImGuiUtility.InputText("", rootPathBuffer, out var inputRootPath);
                        if (ImGui.Button("Load"))
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
            var installation = new GameInstallation(new AptEditorDefinition(), rootPath);
            using var game = new Game(installation, null, new Configuration { LoadShellMap = false });
            // should be the ImGui context created by DeveloperModeView
            var initialContext = ImGui.GetCurrentContext();
            var device = game.GraphicsDevice;
            var window = game.Window;
            using var imGuiRenderer = new ImGuiRenderer(device,
                                                        game.Panel.OutputDescription,
                                                        window.ClientBounds.Width,
                                                        window.ClientBounds.Height);
            using var commandList = device.ResourceFactory.CreateCommandList();
            var ourContext = ImGui.GetCurrentContext();
            // reset ImGui Context to initial one
            ImGui.SetCurrentContext(initialContext);

            using var mainForm = new MainForm(game);
            void OnClientSizeChanged(object? sender, EventArgs args)
            {
                imGuiRenderer.WindowResized(window.ClientBounds.Width, window.ClientBounds.Height);
            }
            void OnRendering2D(object? sender, EventArgs e)
            {
                var previousContext = ImGui.GetCurrentContext();
                ImGui.SetCurrentContext(ourContext);
                try
                {
                    commandList.Begin();
                    commandList.SetFramebuffer(game.Panel.Framebuffer);
                    imGuiRenderer.Update((float) game.RenderTime.DeltaTime.TotalSeconds, window.CurrentInputSnapshot);
                    mainForm.Draw();
                    imGuiRenderer.Render(game.GraphicsDevice, commandList);
                    commandList.End();
                    device.SubmitCommands(commandList);
                }
                finally
                {
                    ImGui.SetCurrentContext(previousContext);
                }
            }
            window.ClientSizeChanged += OnClientSizeChanged;
            game.RenderCompleted += OnRendering2D;
            try
            {
                game.ShowMainMenu();
                game.Run();
            }
            finally
            {
                game.RenderCompleted -= OnRendering2D;
                window.ClientSizeChanged -= OnClientSizeChanged;
            }
        }

        static void Main(string[] args)
        {
            Platform.Start();

            var rootPath = Launcher();
            if (rootPath != null)
            {
                GameWindowAdapter(rootPath);
            }

            Platform.Stop();
        }
    }
}
