#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: 
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using LLGfx.Hosting.Win32;

namespace LLGfx.Hosting
{
    /// <summary>
    /// A control that enables graphics rendering inside a WPF control through
    /// the use of a hosted child Hwnd.
    /// </summary>
    public abstract class HwndWrapper : HwndHost
    {
        #region Fields

        private readonly DpiScale _dpiScale;

        // The name of our window class
        private const string WindowClass = "GraphicsDeviceControlHostWindowClass";

        // The HWND we present to when rendering
        private IntPtr _hWnd;

        // For holding previous hWnd focus
        private IntPtr _hWndPrev;

        // Track if the application has focus
        private bool _applicationHasFocus;

        // Track if the mouse is in the window
        private bool _mouseInWindow;

        // Track the previous mouse position
        private Point _previousPosition;

        // Track the mouse state
        private readonly HwndMouseState _mouseState = new HwndMouseState();

        // Tracking whether we've "capture" the mouse
        private bool _isMouseCaptured;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the control receives a left mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDown;

        /// <summary>
        /// Invoked when the control receives a left mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonUp;

        /// <summary>
        /// Invoked when the control receives a left mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a right mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDown;

        /// <summary>
        /// Invoked when the control receives a right mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonUp;

        /// <summary>
        /// Invoked when the control receives a rigt mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a middle mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDown;

        /// <summary>
        /// Invoked when the control receives a middle mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonUp;

        /// <summary>
        /// Invoked when the control receives a middle mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseMove;

        /// <summary>
        /// Invoked when the control first gets a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseEnter;

        /// <summary>
        /// Invoked when the control gets a mouse leave message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseLeave;

        /// <summary>
        /// Invoked when the control recieves a mouse wheel delta.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseWheel;

        #endregion

        #region Properties

        public new bool IsMouseCaptured
        {
            get { return _isMouseCaptured; }
        }

        #endregion

        #region Construction and Disposal

        protected HwndWrapper()
        {
            Unloaded += OnUnloaded;

            // We must be notified of the application foreground status for our mouse input events
            Application.Current.Activated += OnApplicationActivated;
            Application.Current.Deactivated += OnApplicationDeactivated;

            // Check whether the application is active (it almost certainly is, at this point).
            if (Application.Current.Windows.Cast<Window>().Any(x => x.IsActive))
                _applicationHasFocus = true;

            _dpiScale = VisualTreeHelper.GetDpi(this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            // Unhook all events.
            if (Application.Current != null)
            {
                Application.Current.Activated -= OnApplicationActivated;
                Application.Current.Deactivated -= OnApplicationDeactivated;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Captures the mouse, hiding it and trapping it inside the window bounds.
        /// </summary>
        /// <remarks>
        /// This method is useful for tooling scenarios where you only care about the mouse deltas
        /// and want the user to be able to continue interacting with the window while they move
        /// the mouse. A good example of this is rotating an object based on the mouse deltas where
        /// through capturing you can spin and spin without having the cursor leave the window.
        /// </remarks>
        public new void CaptureMouse()
        {
            // Don't do anything if the mouse is already captured
            if (_isMouseCaptured)
                return;

            NativeMethods.SetCapture(_hWnd);
            _isMouseCaptured = true;
        }

        /// <summary>
        /// Releases the capture of the mouse which makes it visible and allows it to leave the window bounds.
        /// </summary>
        public new void ReleaseMouseCapture()
        {
            // Don't do anything if the mouse is not captured
            if (!_isMouseCaptured)
                return;

            NativeMethods.ReleaseCapture();
            _isMouseCaptured = false;
        }

        #endregion

        #region Graphics Device Control Implementation

        private void OnApplicationActivated(object sender, EventArgs e)
        {
            _applicationHasFocus = true;
        }

        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            _applicationHasFocus = false;
            ResetMouseState();

            if (_mouseInWindow)
            {
                _mouseInWindow = false;
                RaiseHwndMouseLeave(new HwndMouseEventArgs(_mouseState));
            }

            ReleaseMouseCapture();
        }

        private void ResetMouseState()
        {
            // We need to invoke events for any buttons that were pressed
            bool fireL = _mouseState.LeftButton == MouseButtonState.Pressed;
            bool fireM = _mouseState.MiddleButton == MouseButtonState.Pressed;
            bool fireR = _mouseState.RightButton == MouseButtonState.Pressed;
            bool fireX1 = _mouseState.X1Button == MouseButtonState.Pressed;
            bool fireX2 = _mouseState.X2Button == MouseButtonState.Pressed;

            // Update the state of all of the buttons
            _mouseState.LeftButton = MouseButtonState.Released;
            _mouseState.MiddleButton = MouseButtonState.Released;
            _mouseState.RightButton = MouseButtonState.Released;
            _mouseState.X1Button = MouseButtonState.Released;
            _mouseState.X2Button = MouseButtonState.Released;

            // Fire any events
            var args = new HwndMouseEventArgs(_mouseState);
            if (fireL)
                RaiseHwndLButtonUp(args);
            if (fireM)
                RaiseHwndMButtonUp(args);
            if (fireR)
                RaiseHwndRButtonUp(args);
            if (fireX1)
                RaiseHwndX1ButtonUp(args);
            if (fireX2)
                RaiseHwndX2ButtonUp(args);

            // The mouse is no longer considered to be in our window
            _mouseInWindow = false;
        }

        #endregion

        #region HWND Management

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // Create the host window as a child of the parent
            _hWnd = CreateHostWindow(hwndParent.Handle);
            return new HandleRef(this, _hWnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // Destroy the window and reset our hWnd value
            NativeMethods.DestroyWindow(hwnd.Handle);
            _hWnd = IntPtr.Zero;
        }

        /// <summary>
        /// Creates the host window as a child of the parent window.
        /// </summary>
        private IntPtr CreateHostWindow(IntPtr hWndParent)
        {
            // Register our window class
            RegisterWindowClass();

            // Create the window
            return NativeMethods.CreateWindowEx(0, WindowClass, "",
               NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
               0, 0, (int) Width, (int) Height, hWndParent, IntPtr.Zero, IntPtr.Zero, 0);
        }

        /// <summary>
        /// Registers the window class.
        /// </summary>
        private void RegisterWindowClass()
        {
            var wndClass = new NativeMethods.WNDCLASSEX();
            wndClass.cbSize = (uint) Marshal.SizeOf(wndClass);
            wndClass.hInstance = NativeMethods.GetModuleHandle(null);
            wndClass.lpfnWndProc = NativeMethods.DefaultWindowProc;
            wndClass.lpszClassName = WindowClass;
            wndClass.hCursor = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW);

            NativeMethods.RegisterClassEx(ref wndClass);
        }

        #endregion

        #region WndProc Implementation

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeMethods.WM_MOUSEWHEEL:
                    if (_mouseInWindow)
                    {
                        int delta = NativeMethods.GetWheelDeltaWParam(wParam.ToInt64());
                        RaiseHwndMouseWheel(new HwndMouseEventArgs(_mouseState, delta, 0));
                    }
                    break;
                case NativeMethods.WM_LBUTTONDOWN:
                    _mouseState.LeftButton = MouseButtonState.Pressed;
                    RaiseHwndLButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_LBUTTONUP:
                    _mouseState.LeftButton = MouseButtonState.Released;
                    RaiseHwndLButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_LBUTTONDBLCLK:
                    RaiseHwndLButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.Left));
                    break;
                case NativeMethods.WM_RBUTTONDOWN:
                    _mouseState.RightButton = MouseButtonState.Pressed;
                    RaiseHwndRButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_RBUTTONUP:
                    _mouseState.RightButton = MouseButtonState.Released;
                    RaiseHwndRButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_RBUTTONDBLCLK:
                    RaiseHwndRButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.Right));
                    break;
                case NativeMethods.WM_MBUTTONDOWN:
                    _mouseState.MiddleButton = MouseButtonState.Pressed;
                    RaiseHwndMButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_MBUTTONUP:
                    _mouseState.MiddleButton = MouseButtonState.Released;
                    RaiseHwndMButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_MBUTTONDBLCLK:
                    RaiseHwndMButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.Middle));
                    break;
                case NativeMethods.WM_XBUTTONDOWN:
                    if (((int) wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        _mouseState.X1Button = MouseButtonState.Pressed;
                        RaiseHwndX1ButtonDown(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (((int) wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        _mouseState.X2Button = MouseButtonState.Pressed;
                        RaiseHwndX2ButtonDown(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONUP:
                    if (((int) wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        _mouseState.X1Button = MouseButtonState.Released;
                        RaiseHwndX1ButtonUp(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (((int) wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        _mouseState.X2Button = MouseButtonState.Released;
                        RaiseHwndX2ButtonUp(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONDBLCLK:
                    if (((int) wParam & NativeMethods.MK_XBUTTON1) != 0)
                        RaiseHwndX1ButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.XButton1));
                    else if (((int) wParam & NativeMethods.MK_XBUTTON2) != 0)
                        RaiseHwndX2ButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.XButton2));
                    break;
                case NativeMethods.WM_MOUSEMOVE:
                    // If the application isn't in focus, we don't handle this message
                    if (!_applicationHasFocus)
                        break;

                    // record the prevous and new position of the mouse
                    _mouseState.ScreenPosition = PointToScreen(new Point(
                        NativeMethods.GetXLParam((int) lParam) / _dpiScale.DpiScaleX,
                        NativeMethods.GetYLParam((int) lParam) / _dpiScale.DpiScaleY));

                    if (!_mouseInWindow)
                    {
                        _mouseInWindow = true;

                        RaiseHwndMouseEnter(new HwndMouseEventArgs(_mouseState));

                        // Track the previously focused window, and set focus to this window.
                        _hWndPrev = NativeMethods.GetFocus();
                        NativeMethods.SetFocus(_hWnd);

                        // send the track mouse event so that we get the WM_MOUSELEAVE message
                        var tme = new NativeMethods.TRACKMOUSEEVENT
                        {
                            cbSize = Marshal.SizeOf(typeof(NativeMethods.TRACKMOUSEEVENT)),
                            dwFlags = NativeMethods.TME_LEAVE,
                            hWnd = hwnd
                        };
                        NativeMethods.TrackMouseEvent(ref tme);
                    }

                    if (_mouseState.ScreenPosition != _previousPosition)
                        RaiseHwndMouseMove(new HwndMouseEventArgs(_mouseState));

                    _previousPosition = _mouseState.ScreenPosition;

                    break;
                case NativeMethods.WM_MOUSELEAVE:

                    // If we have capture, we ignore this message because we're just
                    // going to reset the cursor position back into the window
                    if (_isMouseCaptured)
                        break;

                    // Reset the state which releases all buttons and 
                    // marks the mouse as not being in the window.
                    ResetMouseState();

                    RaiseHwndMouseLeave(new HwndMouseEventArgs(_mouseState));

                    NativeMethods.SetFocus(_hWndPrev);

                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected virtual void RaiseHwndLButtonDown(HwndMouseEventArgs args)
        {
            HwndLButtonDown?.Invoke(this, args);
        }

        protected virtual void RaiseHwndLButtonUp(HwndMouseEventArgs args)
        {
            HwndLButtonUp?.Invoke(this, args);
        }

        protected virtual void RaiseHwndRButtonDown(HwndMouseEventArgs args)
        {
            HwndRButtonDown?.Invoke(this, args);
        }

        protected virtual void RaiseHwndRButtonUp(HwndMouseEventArgs args)
        {
            HwndRButtonUp?.Invoke(this, args);
        }

        protected virtual void RaiseHwndMButtonDown(HwndMouseEventArgs args)
        {
            HwndMButtonDown?.Invoke(this, args);
        }

        protected virtual void RaiseHwndMButtonUp(HwndMouseEventArgs args)
        {
            HwndMButtonUp?.Invoke(this, args);
        }

        protected virtual void RaiseHwndLButtonDblClick(HwndMouseEventArgs args)
        {
            HwndLButtonDblClick?.Invoke(this, args);
        }

        protected virtual void RaiseHwndRButtonDblClick(HwndMouseEventArgs args)
        {
            HwndRButtonDblClick?.Invoke(this, args);
        }

        protected virtual void RaiseHwndMButtonDblClick(HwndMouseEventArgs args)
        {
            HwndMButtonDblClick?.Invoke(this, args);
        }

        protected virtual void RaiseHwndMouseEnter(HwndMouseEventArgs args)
        {
            HwndMouseEnter?.Invoke(this, args);
        }

        protected virtual void RaiseHwndX1ButtonDown(HwndMouseEventArgs args)
        {
            HwndX1ButtonDown?.Invoke(this, args);
        }

        protected virtual void RaiseHwndX1ButtonUp(HwndMouseEventArgs args)
        {
            HwndX1ButtonUp?.Invoke(this, args);
        }

        protected virtual void RaiseHwndX2ButtonDown(HwndMouseEventArgs args)
        {
            HwndX2ButtonDown?.Invoke(this, args);
        }

        protected virtual void RaiseHwndX2ButtonUp(HwndMouseEventArgs args)
        {
            HwndX2ButtonUp?.Invoke(this, args);
        }

        protected virtual void RaiseHwndX1ButtonDblClick(HwndMouseEventArgs args)
        {
            HwndX1ButtonDblClick?.Invoke(this, args);
        }

        protected virtual void RaiseHwndX2ButtonDblClick(HwndMouseEventArgs args)
        {
            HwndX2ButtonDblClick?.Invoke(this, args);
        }

        protected virtual void RaiseHwndMouseLeave(HwndMouseEventArgs args)
        {
            HwndMouseLeave?.Invoke(this, args);
        }

        protected virtual void RaiseHwndMouseMove(HwndMouseEventArgs args)
        {
            HwndMouseMove?.Invoke(this, args);
        }

        protected virtual void RaiseHwndMouseWheel(HwndMouseEventArgs args)
        {
            HwndMouseWheel?.Invoke(this, args);
        }

        #endregion
    }
}
