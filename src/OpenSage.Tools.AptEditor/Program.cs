using System;
using System.Diagnostics;
using SharpFileDialog;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Tools.AptEditor.UI;
using OpenSage.Tools.AptEditor.Util;
using Veldrid;
using Veldrid.StartupUtilities;
using System.Threading.Tasks;

namespace OpenSage.Tools.AptEditor
{
    class Program
    {
        
        static void Main(string[] args)
        {

            Platform.Start();

            var rootPath = Launcher();
            if (rootPath != null)
            {
                GWA(rootPath);
            }

            Platform.Stop();
        }

        static string? Launcher()
        {

            var createInfo = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 400,
                WindowHeight = 150,
                WindowInitialState = Veldrid.WindowState.Normal,
                WindowTitle = "OpenSAGE Apt Editor"
            };
            VeldridStartup.CreateWindowAndGraphicsDevice(createInfo, out var window, out var outGraphicsDevice);
            using var graphicsDevice = outGraphicsDevice;
            graphicsDevice.SyncToVerticalBlank = true;
            using var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            using var imGuiRenderer = new ImGuiRenderer(graphicsDevice,
                                                        graphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                                                        window.Width,
                                                        window.Height);
            using var gameTimer = new DeltaTimer();
            string? rootPath = null;
            var inputRootPath = "G:\\Games\\RA#s\\aptuis\\aptui"; // TODO I wanted to cache it, but it turned out to be complicated

            void OnWindowResized()
            {
                graphicsDevice.ResizeMainWindow((uint) window.Width, (uint) window.Height);
                imGuiRenderer.WindowResized(window.Width, window.Height);
            }
            window.Resized += OnWindowResized;
            try
            {
                gameTimer.Start();
                while (rootPath == null)
                {
                    if (!window.Exists)
                    {
                        return null;
                    }

                    gameTimer.Update();
                    var inputSnapshot = window.PumpEvents();
                    imGuiRenderer.Update((float) gameTimer.CurrentGameTime.DeltaTime.TotalSeconds, inputSnapshot);

                    commandList.Begin();
                    commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);
                    commandList.ClearColorTarget(0, RgbaFloat.Clear);

                    ImGui.SetNextWindowPos(Vector2.Zero);
                    ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));
                    var open = true;
                    if (ImGui.Begin("Launcher", ref open, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize))
                    {
                        ImGui.TextWrapped("Start OpenSage Apt Editor by providing a root path.");
                        ImGui.TextWrapped("It's recommended that you set it to " +
                            "the RA3SDK_UI_ScreensPack folder so you could load apt files more easily.");

                        ImGui.InputText("##rootPath", ref inputRootPath, 1024);

                        ImGui.SameLine();
                        if (ImGui.Button("..."))
                        {
                            var dirDialog = new DirectoryDialog("Select Root Path");
                            dirDialog.Open(result => inputRootPath = result.FileName);
                        }
                        if (ImGui.Button("Load"))
                        {
                            rootPath = inputRootPath;
                            window.Close();
                        }
                    }
                    ImGui.End();

                    imGuiRenderer.Render(graphicsDevice, commandList);
                    commandList.End();
                    graphicsDevice.SubmitCommands(commandList);
                    graphicsDevice.SwapBuffers();
                }
                return rootPath;
            }
            finally
            {
                window.Resized -= OnWindowResized;
            }
        }

        static void GWA(string rootPath)
        {
            ImGuiRenderer? imGuiRenderer = null;
            CommandList? commandList = null;
            EventHandler? OnClientSizeChanged = null;
            EventHandler? OnRendering2D = null;
            void Attach(Game game)
            {
                var device = game.GraphicsDevice;
                var window = game.Window;
                var mainForm = new MainForm(game);

                var initialContext = ImGui.GetCurrentContext();
                imGuiRenderer = new ImGuiRenderer(device,
                                                            game.Panel.OutputDescription,
                                                            window.ClientBounds.Width,
                                                            window.ClientBounds.Height);
                var font = imGuiRenderer.LoadSystemFont("consola.ttf");

                commandList = device.ResourceFactory.CreateCommandList();
                var ourContext = ImGui.GetCurrentContext();
                // reset ImGui Context to initial one
                ImGui.SetCurrentContext(initialContext);


                OnClientSizeChanged = (a, b) =>
                {
                    imGuiRenderer.WindowResized(window.ClientBounds.Width, window.ClientBounds.Height);
                };
                OnRendering2D = (a, b) =>
                {
                    var previousContext = ImGui.GetCurrentContext();
                    ImGui.SetCurrentContext(ourContext);
                    try
                    {
                        commandList.Begin();
                        commandList.SetFramebuffer(game.Panel.Framebuffer);
                        imGuiRenderer.Update((float) game.RenderTime.DeltaTime.TotalSeconds, window.CurrentInputSnapshot);
                        using (var fontSetter = new ImGuiFontSetter(font))
                        {
                            mainForm.Draw();
                        }
                        imGuiRenderer.Render(game.GraphicsDevice, commandList);
                        commandList.End();
                        device.SubmitCommands(commandList);
                    }
                    finally
                    {
                        ImGui.SetCurrentContext(previousContext);
                    }
                };
                window.ClientSizeChanged += OnClientSizeChanged;
                game.RenderCompleted += OnRendering2D;
            }
            void Detach(Game game)
            {
                var window = game.Window;
                game.RenderCompleted -= OnRendering2D;
                window.ClientSizeChanged -= OnClientSizeChanged;
                imGuiRenderer!.Dispose();
                commandList!.Dispose();
            }

            using var editor = new AptEditor();
            editor.AddSearchPath(rootPath);
            editor.InitConsole(Attach, Detach);
        }

        static void GameWindowAdapter(string rootPath)
        {
            var installation = new GameInstallation(new AptEditorDefinition(), rootPath);
            using var game = new Game(installation, null, new Configuration { LoadShellMap = false });
            // should be the ImGui context created by DeveloperModeView

            
            var device = game.GraphicsDevice;
            var window = game.Window;
            var mainForm = new MainForm(game);

            var initialContext = ImGui.GetCurrentContext();
            using var imGuiRenderer = new ImGuiRenderer(device,
                                                        game.Panel.OutputDescription,
                                                        window.ClientBounds.Width,
                                                        window.ClientBounds.Height);
            var font = imGuiRenderer.LoadSystemFont("consola.ttf");

            using var commandList = device.ResourceFactory.CreateCommandList();
            var ourContext = ImGui.GetCurrentContext();
            // reset ImGui Context to initial one
            ImGui.SetCurrentContext(initialContext);

            
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
                    using (var fontSetter = new ImGuiFontSetter(font))
                    {
                        mainForm.Draw();
                    }
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
    }
}
