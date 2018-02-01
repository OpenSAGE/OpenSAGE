using OpenSage.Input;
using System.Numerics;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndInputMessageHandler : InputMessageHandler
    {
        private readonly WndWindowManager _windowManager;

        private WndWindow _lastHighlightedElement;

        public WndInputMessageHandler(WndWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            var context = new UIElementCallbackContext(_windowManager);

            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    {
                        var element = _windowManager.FindWindow(new Vector2(message.MouseX.Value, message.MouseY.Value));
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
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseDown:
                    {
                        var element = _windowManager.FindWindow(new Vector2(message.MouseX.Value, message.MouseY.Value));
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseDown, element),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseUp:
                    {
                        var element = _windowManager.FindWindow(new Vector2(message.MouseX.Value, message.MouseY.Value));
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseUp, element),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.KeyDown:
                    {
                        if (message.Key == Key.Escape && _windowManager.OpenWindowCount > 1)
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
