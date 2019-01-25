using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class DeveloperModeView : DisposableBase
    {
        private readonly Game _game;
        private readonly ImGuiRenderer _imGuiRenderer;

        private readonly CommandList _commandList;

        private readonly InputSnapshot _emptyInputSnapshot = new EmptyInputSnapshot();
        private bool _isGameViewFocused = false;

        private readonly MainView _mainView;

        public DeveloperModeView(Game game)
        {
            _game = game;

            var window = game.Window;

            _imGuiRenderer = AddDisposable(new ImGuiRenderer(
                window.GraphicsDevice,
                window.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                window.ClientBounds.Width,
                window.ClientBounds.Height));

            void OnWindowSizeChanged(object sender, EventArgs e)
            {
                _imGuiRenderer.WindowResized(window.ClientBounds.Width, window.ClientBounds.Height);
            }

            window.ClientSizeChanged += OnWindowSizeChanged;

            AddDisposeAction(() => window.ClientSizeChanged -= OnWindowSizeChanged);

            _commandList = AddDisposable(window.GraphicsDevice.ResourceFactory.CreateCommandList());

            _mainView = AddDisposable(new MainView(new DiagnosticViewContext(game, _imGuiRenderer)));
        }

        public void Tick()
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(_game.GraphicsDevice.MainSwapchain.Framebuffer);

            _commandList.ClearColorTarget(0, RgbaFloat.Clear);

            if (_isGameViewFocused)
            {
                if (_game.Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.Escape))
                {
                    _isGameViewFocused = false;
                }
            }

            var inputSnapshot = _isGameViewFocused
                ? _emptyInputSnapshot
                : _game.Window.CurrentInputSnapshot;

            _imGuiRenderer.Update(
                (float) _game.RenderTime.DeltaTime.TotalSeconds,
                inputSnapshot);

            _mainView.Draw(ref _isGameViewFocused);

            _imGuiRenderer.Render(_game.GraphicsDevice, _commandList);

            _commandList.End();

            _game.GraphicsDevice.SubmitCommands(_commandList);
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
