using Veldrid;
using Veldrid.StartupUtilities;
using OpenSage.Tools.BigEditor.UI;

namespace OpenSage.Tools.BigEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Platform.Start();

            const int initialWidth = 1024;
            const int initialHeight = 768;

            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(100, 100, initialWidth, initialHeight, WindowState.Normal, "OpenSAGE Big Editor"),
                out var window,
                out var graphicsDevice);

            graphicsDevice.SyncToVerticalBlank = true;

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

                using (var mainForm = new MainForm())
                {
                    while (windowOpen)
                    {
                        commandList.Begin();

                        gameTimer.Update();

                        var inputSnapshot = window.PumpEvents();

                        commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);

                        commandList.ClearColorTarget(0, RgbaFloat.Clear);

                        imGuiRenderer.Update(
                            (float) gameTimer.CurrentGameTime.DeltaTime.TotalSeconds,
                            inputSnapshot);

                        mainForm.Draw(window);

                        imGuiRenderer.Render(graphicsDevice, commandList);

                        commandList.End();

                        graphicsDevice.SubmitCommands(commandList);

                        graphicsDevice.SwapBuffers();
                    }
                }
            }

            graphicsDevice.Dispose();

            Platform.Stop();
        }
    }
}
