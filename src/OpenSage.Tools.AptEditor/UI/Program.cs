using System;
using System.Diagnostics;
using SharpFileDialog;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;
using OpenSage.Tools.AptEditor;

namespace OpenSage.Tools.AptEditor.UI
{
    class Program
    {
        
        static void Main(string[] args)
        {
            // SampleApt.Test1Async("G:\\1\\hkbk.apt"); SampleApt.Test2("G:\\2\\main_mouse.apt");
            //SampleApt.Test3(); return;
            Platform.Start();

            var rootPath = Launcher();
            if (rootPath != null)
            {
                using var editor = new MainForm(rootPath);
                while (true)
                    if (!editor.Scene.Game.RunOnce())
                        break;
                // MT
                // using var editor = new AptSceneInstanceMT(rootPath, null, Attach, Detach);
                // editor.TheTask.Wait();
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

    }
}
