using OpenSage.Gui.Wnd.Elements;
using OpenSage.Input;
using OpenSage.LowLevel.Input;
using System.Numerics;
using System.Linq;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndInputMessageHandler : InputMessageHandler
    {
        private readonly WndSystem _guiSystem;

        private UIElement _lastHighlightedElement;

        public WndInputMessageHandler(WndSystem guiSystem)
        {
            _guiSystem = guiSystem;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            var context = new UIElementCallbackContext(_guiSystem);

            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    {
                        var element = FindElement(new Vector2(message.MouseX.Value, message.MouseY.Value));
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
                        var element = FindElement(new Vector2(message.MouseX.Value, message.MouseY.Value));
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
                        var element = FindElement(new Vector2(message.MouseX.Value, message.MouseY.Value));
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

        private UIElement FindElement(Vector2 mousePosition)
        {
            if (_guiSystem.WindowStack.Count == 0)
            {
                return null;
            }

            var window = _guiSystem.WindowStack.Peek();

            if (window == null)
            {
                return null;
            }

            // Finds deepest element that is visible and contains mousePosition.
            UIElement findElementRecursive(UIElement element)
            {
                if (!element.Visible || !element.Frame.Contains(mousePosition))
                {
                    return null;
                }

                foreach (var child in element.Children.Reverse())
                {
                    var found = findElementRecursive(child);
                    if (found != null)
                    {
                        return found;
                    }
                }

                return element;
            }

            return findElementRecursive(window.Root);
        }
    }
}
