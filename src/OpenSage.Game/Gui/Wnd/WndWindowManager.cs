using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowManager
    {
        private readonly Game _game;

        public int OpenWindowCount => WindowStack.Count;

        internal Stack<Window> WindowStack { get; }

        public WindowTransitionManager TransitionManager { get; }
        public Control FocussedControl { get; private set; }

        public Window TopWindow => WindowStack.Count > 0 ? WindowStack.Peek() : null;

        public WndWindowManager(Game game)
        {
            _game = game;
            WindowStack = new Stack<Window>();

            game.InputMessageBuffer.Handlers.Add(new WndInputMessageHandler(this, _game));

            TransitionManager = new WindowTransitionManager(game.AssetStore.WindowTransitions);
            FocussedControl = null;
        }

        public Window PushWindow(Window window)
        {
            window.Size = _game.Panel.ClientBounds.Size;

            WindowStack.Push(window);

            window.LayoutInit?.Invoke(window, _game);

            FocussedControl = null;

            return window;
        }

        public Window PushWindow(string wndFileName, object tag = null)
        {
            var window = _game.LoadWindow(wndFileName);
            window.Tag = tag;
            return PushWindow(window);
        }

        public Window SetWindow(string wndFileName, object tag = null)
        {
            // TODO: Handle transitions between windows.

            while (WindowStack.Count > 0)
            {
                PopWindow();
            }

            return PushWindow(wndFileName, tag);
        }

        internal void OnViewportSizeChanged(in Size newSize)
        {
            foreach (var window in WindowStack)
            {
                window.Size = newSize;
            }
        }

        public void PopWindow()
        {
            var popped = WindowStack.Pop();
            popped.Dispose();
        }

        public void PopWindow(string windowName)
        {
            var window = WindowStack.Peek();

            if (window.Name == windowName)
            {
                WindowStack.Pop();
                window.Dispose();
            }
        }

        private Window PrepareMessageBox(string title, string text)
        {
            var messageBox = PushWindow(@"Menus\MessageBox.wnd");
            messageBox.Controls.FindControl("MessageBox.wnd:StaticTextTitle").Text = title;
            var staticTextTitle = messageBox.Controls.FindControl("MessageBox.wnd:StaticTextTitle") as Label;
            staticTextTitle.TextAlignment = TextAlignment.Leading;

            messageBox.Controls.FindControl("MessageBox.wnd:StaticTextMessage").Text = text;

            return messageBox;
        }

        public void ShowMessageBox(string title, string text)
        {
            var messageBox = this.PrepareMessageBox(title, text);

            messageBox.Controls.FindControl("MessageBox.wnd:ButtonOk").Show();
        }

        public void ShowDialogBox(string title, string text, out Control yesButton, out Control noButton)
        {
            var messageBox = this.PrepareMessageBox(title, text);
            yesButton = messageBox.Controls.FindControl("MessageBox.wnd:ButtonYes");
            yesButton.Show();
            noButton = messageBox.Controls.FindControl("MessageBox.wnd:ButtonNo");
            noButton.Show();
        }

        public Control GetControlAtPoint(in Point2D mousePosition)
        {
            if (WindowStack.Count == 0)
            {
                return null;
            }

            var windowArray = WindowStack.ToArray();
            foreach(var window in windowArray)
            {
                var control = window.GetSelfOrDescendantAtPoint(mousePosition);
                if(control != null && control != window)
                {
                    return control;
                }
            }

            return null;
        }

        internal void Focus(Control control)
        {
            FocussedControl = control;
        }

        public Control[] GetControlsAtPoint(in Point2D mousePosition)
        {
            if (WindowStack.Count == 0)
            {
                return Array.Empty<Control>();
            }

            var window = WindowStack.Peek();

            return window.GetSelfOrDescendantsAtPoint(mousePosition);
        }

        internal void Update(in TimeInterval gameTime)
        {
            foreach (var window in WindowStack)
            {
                window.LayoutUpdate?.Invoke(window, _game);
            }

            TransitionManager.Update(gameTime);

            foreach (var window in WindowStack)
            {
                window.Update();
            }
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            // TODO: Try to avoid using LINQ here.
            foreach (var window in WindowStack.Reverse())
            {
                window.Render(drawingContext);
            }
        }
    }
}
