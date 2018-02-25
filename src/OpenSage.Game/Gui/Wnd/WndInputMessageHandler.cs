using System.Numerics;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndInputMessageHandler : InputMessageHandler
    {
        private readonly WndWindowManager _windowManager;
        private readonly Game _game;

        private Control _lastHighlightedElement;

        public WndInputMessageHandler(WndWindowManager windowManager, Game game)
        {
            _windowManager = windowManager;
            _game = game;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            var context = new ControlCallbackContext(_windowManager, _game);

            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    {
                        var element = _windowManager.FindControl(message.Value.MousePosition);
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
                            var mousePosition = element?.PointToClient(message.Value.MousePosition);
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseMove, element, mousePosition),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseLeftButtonDown:
                    {
                        var element = _windowManager.FindControl(message.Value.MousePosition);
                        if (element != null)
                        {
                            var mousePosition = element.PointToClient(message.Value.MousePosition);
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseDown, element, mousePosition),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseLeftButtonUp:
                    {
                        var element = _windowManager.FindControl(message.Value.MousePosition);
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseUp, element, message.Value.MousePosition),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.KeyDown:
                    {
                        if (message.Value.Key == Key.Escape && _windowManager.OpenWindowCount > 1)
                        {
                            _windowManager.PopWindow();
                            return InputMessageResult.Handled;
                        }
                        break;
                    }
            }

            return InputMessageResult.NotHandled;
        }
    }
}
