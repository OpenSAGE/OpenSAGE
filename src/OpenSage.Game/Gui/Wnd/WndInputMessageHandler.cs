using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Mathematics;

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
                        if(mouseOverControls.Count > 0)
                        {
                            _game.Cursors.SetCursor("Arrow", _game.CurrentGameTime);
                            _game.Cursors.IsCursorLocked = true;
                        }
                        else
                        {
                            _game.Cursors.IsCursorLocked = false;
                        }

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

                case InputMessageType.MouseRightButtonDown:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseRightDown, element, mousePosition),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                case InputMessageType.MouseRightButtonUp:
                    {
                        if (GetControlAtPoint(message.Value.MousePosition, out var element, out var mousePosition))
                        {
                            element.InputCallback.Invoke(
                                element,
                                new WndWindowMessage(WndWindowMessageType.MouseRightUp, element, mousePosition),
                                context);
                            return InputMessageResult.Handled;
                        }
                        break;
                    }

                // For the time being, just consume middle click and wheel events so that they don't go through controls:
                case InputMessageType.MouseMiddleButtonDown:
                case InputMessageType.MouseMiddleButtonUp:
                case InputMessageType.MouseWheel:
                    {
                        return GetControlAtPoint(message.Value.MousePosition, out var _, out var _)
                            ? InputMessageResult.Handled
                            : InputMessageResult.NotHandled;
                    }

                case InputMessageType.KeyDown:
                    {
                        var control = _windowManager.FocussedControl;
                        if(control != null)
                        {
                            control?.InputCallback.Invoke(
                                control,
                                new WndWindowMessage(WndWindowMessageType.KeyDown, control, null, message.Value.Key, message.Value.Modifiers),
                                context
                            );
                            return InputMessageResult.Handled;
                        }


                        break;
                    }
            }

            return InputMessageResult.NotHandled;
        }
    }
}
