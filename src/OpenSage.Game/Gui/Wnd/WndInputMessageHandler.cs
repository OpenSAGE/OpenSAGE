using System.Collections.Generic;
using System.Linq;
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

        private readonly List<Control> _lastMouseOverControls = new List<Control>();

        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public WndInputMessageHandler(WndWindowManager windowManager, Game game)
        {
            _windowManager = windowManager;
            _game = game;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            bool GetControlAtPoint(in Point2D mousePosition, out Control control, out Point2D controlRelativePosition)
            {
                control = _windowManager.GetControlAtPoint(mousePosition);

                if (control != null)
                {
                    controlRelativePosition = control.PointToClient(mousePosition);
                    return true;
                }

                controlRelativePosition = Point2D.Zero;;
                return false;
            }

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
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
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
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseUp, element, mousePosition),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                // For the time being, just consume right and middle click events so that they don't go through controls:
                case InputMessageType.MouseRightButtonUp:
                case InputMessageType.MouseRightButtonDown:
                case InputMessageType.MouseMiddleButtonDown:
                case InputMessageType.MouseMiddleButtonUp:
                    {
                        return GetControlAtPoint(message.Value.MousePosition, out var _, out var _)
                            ? InputMessageResult.Handled
                            : InputMessageResult.NotHandled;
                    }

                case InputMessageType.KeyDown:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.KeyDown, element, mousePosition, message.Value.Key),
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
