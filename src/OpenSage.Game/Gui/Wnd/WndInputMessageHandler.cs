using OpenSage.Gui.Wnd.Elements;
using OpenSage.Input;
using OpenSage.LowLevel.Input;
using System.Numerics;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndInputMessageHandler : InputMessageHandler
    {
        private readonly WndWindowManager _windowManager;

        private UIElement _lastHighlightedElement;

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
                        var element = _windowManager.FindElement(new Vector2(message.MouseX.Value, message.MouseY.Value));
                        if (element != _lastHighlightedElement)
                        {
                            if (_lastHighlightedElement != null)
                            {
                                _lastHighlightedElement.InputCallback.Invoke(
                                    _lastHighlightedElement,
                                    new GuiWindowMessage(GuiWindowMessageType.MouseExit, _lastHighlightedElement),
                                    context);
                            }
                            _lastHighlightedElement = element;
                            if (element != null)
                            {
                                element.InputCallback.Invoke(
                                    element,
                                    new GuiWindowMessage(GuiWindowMessageType.MouseEnter, element),
                                    context);
                            }
                        }
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new GuiWindowMessage(GuiWindowMessageType.MouseMove, element),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseDown:
                    {
                        var element = _windowManager.FindElement(new Vector2(message.MouseX.Value, message.MouseY.Value));
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new GuiWindowMessage(GuiWindowMessageType.MouseDown, element),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseUp:
                    {
                        var element = _windowManager.FindElement(new Vector2(message.MouseX.Value, message.MouseY.Value));
                        if (element != null)
                        {
                            element.InputCallback.Invoke(
                                element,
                                new GuiWindowMessage(GuiWindowMessageType.MouseUp, element),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }
            }

            return InputMessageResult.NotHandled;
        }
    }
}
