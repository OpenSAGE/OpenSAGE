using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Wnd;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowPushButton : WndWindow
    {
        private readonly Dictionary<PushButtonState, Texture> _imageTextures;

        private bool _pushed;
        public bool Pushed
        {
            get => _pushed;
            set
            {
                _pushed = value;
                Invalidate();
            }
        }

        private PushButtonState CurrentButtonState
        {
            get
            {
                if (Pushed)
                {
                    return PushButtonState.Pushed;
                }

                switch (CurrentState)
                {
                    case WndWindowState.Enabled:
                        return PushButtonState.Enabled;

                    case WndWindowState.Highlighted:
                        return PushButtonState.Highlighted;

                    case WndWindowState.Disabled:
                        return PushButtonState.Disabled;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        internal WndWindowPushButton(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
            : base(wndWindow, contentManager, callbackResolver)
        {
            _imageTextures = new Dictionary<PushButtonState, Texture>();

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                void createTexture(PushButtonState state, WndDrawData drawData, int leftIndex, int middleIndex, int rightIndex)
                {
                    _imageTextures[state] = contentManager.WndImageTextureCache.GetStretchableTexture(
                        wndWindow,
                        drawData,
                        leftIndex,
                        middleIndex,
                        rightIndex);
                }

                createTexture(PushButtonState.Enabled, wndWindow.EnabledDrawData, 0, 5, 6);
                createTexture(PushButtonState.Highlighted, wndWindow.HiliteDrawData, 0, 5, 6);
                createTexture(PushButtonState.Disabled, wndWindow.DisabledDrawData, 0, 5, 6);
                createTexture(PushButtonState.Pushed, wndWindow.HiliteDrawData, 1, 3, 4);
            }
        }

        protected override void DefaultInputOverride(WndWindowMessage message, UIElementCallbackContext context)
        {
            // TODO: Capture input on mouse down.
            // TODO: Only fire click event when mouse was pressed and released inside same button.

            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseEnter:
                    CurrentState = WndWindowState.Highlighted;
                    break;

                case WndWindowMessageType.MouseExit:
                    CurrentState = WndWindowState.Enabled;
                    break;

                case WndWindowMessageType.MouseDown:
                    Pushed = true;
                    break;

                case WndWindowMessageType.MouseUp:
                    Pushed = false;
                    Parent.SystemCallback.Invoke(
                        this,
                        new WndWindowMessage(WndWindowMessageType.SelectedButton, this),
                        context);
                    break;
            }
        }

        protected override void DefaultDrawOverride(Game game)
        {
            if (_imageTextures.TryGetValue(CurrentButtonState, out var texture) && texture != null)
            {
                PrimitiveBatch.DrawImage(texture, null, Bounds);
            }
        }

        private enum PushButtonState
        {
            Enabled,
            Highlighted,
            Disabled,
            Pushed
        }
    }
}
