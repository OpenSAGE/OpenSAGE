using System.Numerics;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndInputMessageHandler : GameMessageHandler
    {
        private readonly WndWindowManager _windowManager;
        private readonly Game _game;

        private WndWindow _lastHighlightedElement;

        public WndInputMessageHandler(WndWindowManager windowManager, Game game)
        {
            _windowManager = windowManager;
            _game = game;
        }

        public override GameMessageResult HandleMessage(GameMessage message)
        {
            var context = new UIElementCallbackContext(_windowManager, _game);

            switch (message.MessageType)
            {
                case GameMessageType.MouseMove:
                    {
                        var element = _windowManager.FindWindow(message.Arguments[0].Value.ScreenPosition);
                        if (element != _lastHighlightedElement)
                        {
                            if (_lastHighlightedElement != null)
                            {
                                _lastHighlightedElement.InputCallback.Invoke(
                                    _lastHighlightedElement,
                                    new WndWindowMessage(WndWindowMessageType.MouseExit, _lastHighlightedElement),
                                    context);
                            }
                            _lastHighlightedElement = element;
                            if (element != null)
                            {
                                element.InputCallback.Invoke(
                                    element,
                                    new WndWindowMessage(WndWindowMessageType.MouseEnter, element),
                                    context);
                            }
                        }
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseMove, element),
                                context);
                            return GameMessageResult.Handled;
                        }
                        break;
                    }

                case GameMessageType.MouseLeftButtonDown:
                    {
                        var element = _windowManager.FindWindow(message.Arguments[0].Value.ScreenPosition);
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseDown, element),
                                context);
                            return GameMessageResult.Handled;
                        }
                        break;
                    }

                case GameMessageType.MouseLeftButtonUp:
                    {
                        var element = _windowManager.FindWindow(message.Arguments[0].Value.ScreenPosition);
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseUp, element),
                                context);
                            return GameMessageResult.Handled;
                        }
                        break;
                    }

                case GameMessageType.KeyDown:
                    {
                        if ((Key) message.Arguments[0].Value.Integer == Key.Escape && _windowManager.OpenWindowCount > 1)
                        {
                            _windowManager.PopWindow();
                            return GameMessageResult.Handled;
                        }
                        break;
                    }
            }

            return GameMessageResult.NotHandled;
        }
    }
}
