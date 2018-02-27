using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    internal sealed class WndInputMessageHandler : InputMessageHandler
    {
        private readonly WndWindowManager _windowManager;
        private readonly Game _game;

        private readonly List<Control> _lastMouseOverControls = new List<Control>();

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
                        var mouseOverControls = _windowManager.GetControlsAtPoint(message.Value.MousePosition).ToList();
                        foreach (var control in _lastMouseOverControls)
                        {
                            if (!mouseOverControls.Contains(control))
                            {
                                control.InputCallback.Invoke(
                                    control,
                                    new WndWindowMessage(WndWindowMessageType.MouseExit, control),
                                    context);
                            }
                        }
                        foreach (var control in mouseOverControls)
                        {
                            if (!_lastMouseOverControls.Contains(control))
                            {
                                control.InputCallback.Invoke(
                                    control,
                                    new WndWindowMessage(WndWindowMessageType.MouseEnter, control),
                                    context);
                            }
                        }

                        _lastMouseOverControls.Clear();
                        _lastMouseOverControls.AddRange(mouseOverControls);

                        foreach (var control in mouseOverControls)
                        {
                            var mousePosition = control.PointToClient(message.Value.MousePosition);
                            control.InputCallback.Invoke(
                                control,
                                new WndWindowMessage(WndWindowMessageType.MouseMove, control, mousePosition),
                                context);
                        }
                        return mouseOverControls.Count > 0
                            ? InputMessageResult.Handled
                            : InputMessageResult.NotHandled;
                    }

                case InputMessageType.MouseLeftButtonDown:
                    {
                        var element = _windowManager.GetControlAtPoint(message.Value.MousePosition);
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
                        var element = _windowManager.GetControlAtPoint(message.Value.MousePosition);
                        if (element != null)
                        {
                            var mousePosition = element.PointToClient(message.Value.MousePosition);
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseUp, element, mousePosition),
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
