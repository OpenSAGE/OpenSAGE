using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid.Sdl2;

namespace OpenSage
{
    public unsafe sealed class Sdl2GameWindow : GameWindow
    {
        // TODO: Add these to Veldrid.SDL2
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_CreateWindowFrom(IntPtr data);

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_CreateColorCursor(IntPtr surface, int hot_x, int hot_y);

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_FreeCursor(IntPtr cursor);

        public static string SDL_GetError()
        {
            return UTF8_ToManaged(Sdl2Native.SDL_GetError());
        }

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_RaiseWindow(IntPtr window);

        internal static string UTF8_ToManaged(byte* s)
        {
            if (*s == 0)
            {
                return null;
            }

            var ptr = s;
            while (*ptr != 0)
            {
                ptr++;
            }

            var result = System.Text.Encoding.UTF8.GetString(s, (int) (ptr - s));
            
            return result;
        }

        private readonly IntPtr _sdlWindowHandle;

        private Point2D _cachedPosition;
        private Size _cachedSize;

        public override Rectangle ClientBounds => new Rectangle(_cachedPosition, _cachedSize);

        public override bool IsMouseVisible
        {
            get => Sdl2Native.SDL_ShowCursor(Sdl2Native.SDL_QUERY) == 1;
            set => Sdl2Native.SDL_ShowCursor(value ? Sdl2Native.SDL_ENABLE : Sdl2Native.SDL_DISABLE);
        }

        public override IntPtr NativeWindowHandle => GetUnderlyingWindowHandle();

        public Sdl2GameWindow(IntPtr windowsWindowHandle)
        {
            _sdlWindowHandle = SDL_CreateWindowFrom(windowsWindowHandle);

            AfterWindowCreated();
        }

        public Sdl2GameWindow(string title, int x, int y, int width, int height)
        {
            _sdlWindowHandle = Sdl2Native.SDL_CreateWindow(
                title,
                x, y, width, height,
                0);

            AfterWindowCreated();
        }

        private void AfterWindowCreated()
        {
            RefreshCachedPosition();
            RefreshCachedSize();

            Sdl2Native.SDL_ShowWindow(_sdlWindowHandle);
        }

        private IntPtr GetUnderlyingWindowHandle()
        {
            SDL_SysWMinfo wmInfo;
            Sdl2Native.SDL_GetVersion(&wmInfo.version);
            Sdl2Native.SDL_GetWMWindowInfo(_sdlWindowHandle, &wmInfo);

            if (wmInfo.subsystem == SysWMType.Windows)
            {
                var win32Info = Unsafe.Read<Win32WindowInfo>(&wmInfo.info);
                return win32Info.Sdl2Window;
            }

            return IntPtr.Zero;
        }

        public override void SetCursor(Cursor cursor)
        {
            // TODO
        }

        public override bool PumpEvents()
        {
            SDL_Event evt;
            while (Sdl2Native.SDL_PollEvent(&evt) == 1)
            {
                switch (evt.type)
                {
                    case SDL_EventType.Quit:
                        return false;

                    case SDL_EventType.WindowEvent:
                        var windowEvent = Unsafe.Read<SDL_WindowEvent>(&evt);
                        HandleWindowEvent(windowEvent);
                        break;

                    case SDL_EventType.KeyDown:
                    case SDL_EventType.KeyUp:
                        var keyboardEvent = Unsafe.Read<SDL_KeyboardEvent>(&evt);
                        RaiseInputMessageReceived(new InputMessageEventArgs(new InputMessage(
                            keyboardEvent.type == SDL_EventType.KeyDown
                                ? InputMessageType.KeyDown
                                : InputMessageType.KeyUp,
                            MapKey(keyboardEvent.keysym.scancode))));
                        break;

                    case SDL_EventType.TextInput:
                        // TODO
                        break;

                    case SDL_EventType.MouseButtonDown:
                    case SDL_EventType.MouseButtonUp:
                        RaiseMouseButtonEvent(
                            evt.type == SDL_EventType.MouseButtonDown
                                ? InputMessageType.MouseDown
                                : InputMessageType.MouseUp,
                            Unsafe.Read<SDL_MouseButtonEvent>(&evt));
                        break;

                    case SDL_EventType.MouseMotion:
                        RaiseMouseMotionEvent(Unsafe.Read<SDL_MouseMotionEvent>(&evt));
                        break;

                    case SDL_EventType.MouseWheel:
                        RaiseMouseWheelEvent(Unsafe.Read<SDL_MouseWheelEvent>(&evt));
                        break;

                    default:
                        // TODO
                        break;
                }
            }

            return true;
        }

        private void RaiseMouseButtonEvent(
            InputMessageType mouseMessageType,
            SDL_MouseButtonEvent mouseEvent)
        {
            if (mouseMessageType == InputMessageType.MouseDown)
            {
                SDL_RaiseWindow(_sdlWindowHandle);
            }

            RaiseInputMessageReceived(new InputMessageEventArgs(new InputMessage(
                mouseMessageType,
                GetMouseButton(mouseEvent.button),
                mouseEvent.x,
                mouseEvent.y,
                0)));
        }

        private void RaiseMouseMotionEvent(
            SDL_MouseMotionEvent mouseEvent)
        {
            RaiseInputMessageReceived(new InputMessageEventArgs(new InputMessage(
                InputMessageType.MouseMove,
                null,
                mouseEvent.x,
                mouseEvent.y,
                0)));
        }

        private void RaiseMouseWheelEvent(
            SDL_MouseWheelEvent mouseEvent)
        {
            RaiseInputMessageReceived(new InputMessageEventArgs(new InputMessage(
                InputMessageType.MouseWheel,
                null,
                0,
                0,
                mouseEvent.y * 50)));
        }

        private static MouseButton GetMouseButton(SDL_MouseButton button)
        {
            switch (button)
            {
                case SDL_MouseButton.Left:
                    return MouseButton.Left;

                case SDL_MouseButton.Middle:
                    return MouseButton.Middle;

                case SDL_MouseButton.Right:
                    return MouseButton.Right;

                case SDL_MouseButton.X1:
                case SDL_MouseButton.X2:
                default:
                    return MouseButton.Left;
            }
        }

        private void HandleWindowEvent(in SDL_WindowEvent windowEvent)
        {
            switch (windowEvent.@event)
            {
                case SDL_WindowEventID.Resized:
                case SDL_WindowEventID.SizeChanged:
                case SDL_WindowEventID.Minimized:
                case SDL_WindowEventID.Maximized:
                case SDL_WindowEventID.Restored:
                    HandleResizedMessage();
                    break;

                case SDL_WindowEventID.Enter:
                case SDL_WindowEventID.Leave:
                    // TODO
                    break;

                case SDL_WindowEventID.Moved:
                    _cachedPosition = new Point2D(windowEvent.data1, windowEvent.data2);
                    break;

                default:
                    // TODO
                    break;
            }
        }

        private void HandleResizedMessage()
        {
            RefreshCachedSize();
            RaiseClientSizeChanged();
        }

        private void RefreshCachedSize()
        {
            int width, height;
            Sdl2Native.SDL_GetWindowSize(_sdlWindowHandle, &width, &height);
            _cachedSize = new Size(width, height);
        }

        private void RefreshCachedPosition()
        {
            int x, y;
            Sdl2Native.SDL_GetWindowPosition(_sdlWindowHandle, &x, &y);
            _cachedPosition = new Point2D(x, y);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            Sdl2Native.SDL_DestroyWindow(_sdlWindowHandle);

            base.Dispose(disposeManagedResources);
        }

        #region Key mapping

        private static Key MapKey(SDL_Scancode key)
        {
            switch (key)
            {
                case SDL_Scancode.SDL_SCANCODE_BACKSPACE:
                    return Key.Backspace;

                case SDL_Scancode.SDL_SCANCODE_TAB:
                    return Key.Tab;

                case SDL_Scancode.SDL_SCANCODE_RETURN:
                    return Key.Enter;

                case SDL_Scancode.SDL_SCANCODE_ESCAPE:
                    return Key.Escape;

                case SDL_Scancode.SDL_SCANCODE_SPACE:
                    return Key.Space;

                case SDL_Scancode.SDL_SCANCODE_LEFT:
                    return Key.Left;

                case SDL_Scancode.SDL_SCANCODE_UP:
                    return Key.Up;

                case SDL_Scancode.SDL_SCANCODE_RIGHT:
                    return Key.Right;

                case SDL_Scancode.SDL_SCANCODE_DOWN:
                    return Key.Down;

                case SDL_Scancode.SDL_SCANCODE_DELETE:
                    return Key.Delete;

                case SDL_Scancode.SDL_SCANCODE_0:
                    return Key.D0;

                case SDL_Scancode.SDL_SCANCODE_1:
                    return Key.D1;

                case SDL_Scancode.SDL_SCANCODE_2:
                    return Key.D2;

                case SDL_Scancode.SDL_SCANCODE_3:
                    return Key.D3;

                case SDL_Scancode.SDL_SCANCODE_4:
                    return Key.D4;

                case SDL_Scancode.SDL_SCANCODE_5:
                    return Key.D5;

                case SDL_Scancode.SDL_SCANCODE_6:
                    return Key.D6;

                case SDL_Scancode.SDL_SCANCODE_7:
                    return Key.D7;

                case SDL_Scancode.SDL_SCANCODE_8:
                    return Key.D8;

                case SDL_Scancode.SDL_SCANCODE_9:
                    return Key.D9;

                case SDL_Scancode.SDL_SCANCODE_A:
                    return Key.A;

                case SDL_Scancode.SDL_SCANCODE_B:
                    return Key.B;

                case SDL_Scancode.SDL_SCANCODE_C:
                    return Key.C;

                case SDL_Scancode.SDL_SCANCODE_D:
                    return Key.D;

                case SDL_Scancode.SDL_SCANCODE_E:
                    return Key.E;

                case SDL_Scancode.SDL_SCANCODE_F:
                    return Key.F;

                case SDL_Scancode.SDL_SCANCODE_G:
                    return Key.G;

                case SDL_Scancode.SDL_SCANCODE_H:
                    return Key.H;

                case SDL_Scancode.SDL_SCANCODE_I:
                    return Key.I;

                case SDL_Scancode.SDL_SCANCODE_J:
                    return Key.J;

                case SDL_Scancode.SDL_SCANCODE_K:
                    return Key.K;

                case SDL_Scancode.SDL_SCANCODE_L:
                    return Key.L;

                case SDL_Scancode.SDL_SCANCODE_M:
                    return Key.M;

                case SDL_Scancode.SDL_SCANCODE_N:
                    return Key.N;

                case SDL_Scancode.SDL_SCANCODE_O:
                    return Key.O;

                case SDL_Scancode.SDL_SCANCODE_P:
                    return Key.P;

                case SDL_Scancode.SDL_SCANCODE_Q:
                    return Key.Q;

                case SDL_Scancode.SDL_SCANCODE_R:
                    return Key.R;

                case SDL_Scancode.SDL_SCANCODE_S:
                    return Key.S;

                case SDL_Scancode.SDL_SCANCODE_T:
                    return Key.T;

                case SDL_Scancode.SDL_SCANCODE_U:
                    return Key.U;

                case SDL_Scancode.SDL_SCANCODE_V:
                    return Key.V;

                case SDL_Scancode.SDL_SCANCODE_W:
                    return Key.W;

                case SDL_Scancode.SDL_SCANCODE_X:
                    return Key.X;

                case SDL_Scancode.SDL_SCANCODE_Y:
                    return Key.Y;

                case SDL_Scancode.SDL_SCANCODE_Z:
                    return Key.Z;

                case SDL_Scancode.SDL_SCANCODE_KP_0:
                    return Key.NumPad0;

                case SDL_Scancode.SDL_SCANCODE_KP_1:
                    return Key.NumPad1;

                case SDL_Scancode.SDL_SCANCODE_KP_2:
                    return Key.NumPad2;

                case SDL_Scancode.SDL_SCANCODE_KP_3:
                    return Key.NumPad3;

                case SDL_Scancode.SDL_SCANCODE_KP_4:
                    return Key.NumPad4;

                case SDL_Scancode.SDL_SCANCODE_KP_5:
                    return Key.NumPad5;

                case SDL_Scancode.SDL_SCANCODE_KP_6:
                    return Key.NumPad6;

                case SDL_Scancode.SDL_SCANCODE_KP_7:
                    return Key.NumPad7;

                case SDL_Scancode.SDL_SCANCODE_KP_8:
                    return Key.NumPad8;

                case SDL_Scancode.SDL_SCANCODE_KP_9:
                    return Key.NumPad9;

                case SDL_Scancode.SDL_SCANCODE_KP_DIVIDE:
                    return Key.NumPadDivide;

                case SDL_Scancode.SDL_SCANCODE_F1:
                    return Key.F1;

                case SDL_Scancode.SDL_SCANCODE_F2:
                    return Key.F2;

                case SDL_Scancode.SDL_SCANCODE_F3:
                    return Key.F3;

                case SDL_Scancode.SDL_SCANCODE_F4:
                    return Key.F4;

                case SDL_Scancode.SDL_SCANCODE_F5:
                    return Key.F5;

                case SDL_Scancode.SDL_SCANCODE_F6:
                    return Key.F6;

                case SDL_Scancode.SDL_SCANCODE_F7:
                    return Key.F7;

                case SDL_Scancode.SDL_SCANCODE_F8:
                    return Key.F8;

                case SDL_Scancode.SDL_SCANCODE_F9:
                    return Key.F9;

                case SDL_Scancode.SDL_SCANCODE_F10:
                    return Key.F10;

                case SDL_Scancode.SDL_SCANCODE_F11:
                    return Key.F11;

                case SDL_Scancode.SDL_SCANCODE_F12:
                    return Key.F12;

                case SDL_Scancode.SDL_SCANCODE_LCTRL:
                case SDL_Scancode.SDL_SCANCODE_RCTRL:
                    return Key.Ctrl;

                case SDL_Scancode.SDL_SCANCODE_LSHIFT:
                case SDL_Scancode.SDL_SCANCODE_RSHIFT:
                    return Key.Shift;

                case SDL_Scancode.SDL_SCANCODE_LALT:
                case SDL_Scancode.SDL_SCANCODE_RALT:
                    return Key.Alt;

                default:
                    return Key.None;
            }
        }

        #endregion
    }
}
