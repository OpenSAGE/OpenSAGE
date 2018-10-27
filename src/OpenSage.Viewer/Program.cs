using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CommandLine;
using OpenSage.Viewer.UI;
using Veldrid;

namespace OpenSage.Viewer
{
    public static class Program
    {
        //must match Veldrid.GraphicsBackends. Can be removed once nullable enums work
        public enum Renderer : byte
        {
            Direct3D11 = 0,
            Vulkan = 1,
            OpenGL = 2,
            Metal = 3,
            OpenGLES = 4,
            Default
        }

        public class Options
        {
            //use a string since nullable enums aren't working yet
            [Option('r', "renderer", Default = Renderer.Default, Required = false, HelpText = "Set the renderer backend.")]
            public Renderer Renderer { get; set; }
        }

        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts => Run(opts));
        }

        public static void Run(Options opts)
        {
            GraphicsBackend? preferredBackend = null;

            if (opts.Renderer != Renderer.Default)
            {
                preferredBackend = (GraphicsBackend) opts.Renderer;
            }

            Platform.Start();

            const int initialWidth = 1024;
            const int initialHeight = 768;

            using (var window = new GameWindow("OpenSAGE Viewer", 100, 100, initialWidth, initialHeight, preferredBackend))
            using (var commandList = window.GraphicsDevice.ResourceFactory.CreateCommandList())
            using (var imGuiRenderer = new ImGuiRenderer(window.GraphicsDevice, window.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription, initialWidth, initialHeight))
            using (var gameTimer = new GameTimer())
            {
                window.ClientSizeChanged += (sender, e) =>
                {
                    imGuiRenderer.WindowResized(window.ClientBounds.Width, window.ClientBounds.Height);
                };

                gameTimer.Start();

                using (var mainForm = new MainForm(window, imGuiRenderer))
                {
                    var emptyInputSnapshot = new EmptyInputSnapshot();
                    var isGameViewFocused = false;

                    while (true)
                    {
                        commandList.Begin();

                        gameTimer.Update();

                        if (!window.PumpEvents())
                        {
                            break;
                        }

                        commandList.SetFramebuffer(window.GraphicsDevice.MainSwapchain.Framebuffer);

                        commandList.ClearColorTarget(0, RgbaFloat.Clear);

                        if (isGameViewFocused)
                        {
                            if (window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.Escape))
                            {
                                isGameViewFocused = false;
                            }
                        }

                        var inputSnapshot = isGameViewFocused
                            ? emptyInputSnapshot
                            : window.CurrentInputSnapshot;

                        imGuiRenderer.Update(
                            (float) gameTimer.CurrentGameTime.ElapsedGameTime.TotalSeconds,
                            inputSnapshot);

                        mainForm.Draw(ref isGameViewFocused);

                        imGuiRenderer.Render(window.GraphicsDevice, commandList);

                        commandList.End();

                        window.GraphicsDevice.SubmitCommands(commandList);

                        window.GraphicsDevice.SwapBuffers();
                    }
                }
            }

            Platform.Stop();
        }

        private sealed class EmptyInputSnapshot : InputSnapshot
        {
            public IReadOnlyList<KeyEvent> KeyEvents { get; } = Array.Empty<KeyEvent>();

            public IReadOnlyList<MouseEvent> MouseEvents { get; } = Array.Empty<MouseEvent>();

            public IReadOnlyList<char> KeyCharPresses { get; } = Array.Empty<char>();

            public Vector2 MousePosition { get; } = new Vector2(-100, -100);

            public float WheelDelta { get; } = 0;

            public bool IsMouseDown(MouseButton button) => false;
        }
    }
}
