using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Diagnostics.Util;
using OpenSage.Input;
using Veldrid;

namespace OpenSage.Diagnostics
{
    public sealed class DeveloperModeView : DisposableBase
    {
        private readonly Game _game;
        private readonly GameWindow _window;
        private readonly ImGuiRenderer _imGuiRenderer;

        private readonly CommandList _commandList;

        private readonly InputSnapshot _emptyInputSnapshot = new EmptyInputSnapshot();
        private bool _isGameViewFocused = false;

        private readonly MainView _mainView;

        public DeveloperModeView(Game game, GameWindow window)
        {
            _game = game;
            _window = window;

            _imGuiRenderer = AddDisposable(new ImGuiRenderer(
                game.GraphicsDevice,
                game.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                window.ClientBounds.Width,
                window.ClientBounds.Height));

            void OnWindowSizeChanged(object sender, EventArgs e)
            {
                _imGuiRenderer.WindowResized(window.ClientBounds.Width, window.ClientBounds.Height);
            }

            window.ClientSizeChanged += OnWindowSizeChanged;

            AddDisposeAction(() => window.ClientSizeChanged -= OnWindowSizeChanged);

            var inputMessageHandler = new CallbackMessageHandler(
                HandlingPriority.Window,
                message =>
                {
                    if (_isGameViewFocused && message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.Escape)
                    {
                        _isGameViewFocused = false;
                        return InputMessageResult.Handled;
                    }

                    return InputMessageResult.NotHandled;
                });

            game.InputMessageBuffer.Handlers.Add(inputMessageHandler);

            AddDisposeAction(() => game.InputMessageBuffer.Handlers.Remove(inputMessageHandler));

            _commandList = AddDisposable(game.GraphicsDevice.ResourceFactory.CreateCommandList());

            ImGuiUtility.SetupDocking();

            _mainView = AddDisposable(new MainView(new DiagnosticViewContext(game, window, _imGuiRenderer)));
        }

        public void Tick()
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(_window.Swapchain.Framebuffer);

            _commandList.ClearColorTarget(0, RgbaFloat.Clear);

            var inputSnapshot = _isGameViewFocused
                ? _emptyInputSnapshot
                : _window.CurrentInputSnapshot;

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
