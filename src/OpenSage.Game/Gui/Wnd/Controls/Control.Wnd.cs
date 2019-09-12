using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    partial class Control
    {
        public static Control CreateRecursive(
            WndWindowDefinition wndWindow,
            ContentManager contentManager,
            WndCallbackResolver wndCallbackResolver,
            Point2D parentOffset)
        {
            var imageLoader = contentManager.WndImageLoader;

            var result = CreateControl(wndWindow, imageLoader);

            result.Name = wndWindow.Name;

            var wndRectangle = wndWindow.ScreenRect.ToRectangle();
            result.Bounds = new Rectangle(
                wndRectangle.X - parentOffset.X,
                wndRectangle.Y - parentOffset.Y,
                wndRectangle.Width,
                wndRectangle.Height);

            var systemCallback = wndCallbackResolver.GetControlCallback(wndWindow.SystemCallback);
            if (systemCallback != null)
            {
                result.SystemCallback = systemCallback;
            }

            var inputCallback = wndCallbackResolver.GetControlCallback(wndWindow.InputCallback);
            if (inputCallback != null)
            {
                result.InputCallback = inputCallback;
            }

            var drawCallback = wndCallbackResolver.GetControlDrawCallback(wndWindow.DrawCallback);
            if (drawCallback != null)
            {
                result.DrawCallback = drawCallback;
            }

            // TODO: TooltipCallback

            result.Visible = !wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden);

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.SeeThru))
            {
                result.BackgroundColor = ColorRgbaF.Transparent;
                result.BorderColor = ColorRgbaF.Transparent;
            }

            if (wndWindow.HasHeaderTemplate)
            {
                var headerTemplate = contentManager.IniDataContext.HeaderTemplates.First(x => x.Name == wndWindow.HeaderTemplate);
                result.Font = contentManager.FontManager.GetOrCreateFont(headerTemplate.Font, headerTemplate.Point, headerTemplate.Bold ? FontWeight.Bold : FontWeight.Normal);
            }
            else
            {
                result.Font = contentManager.FontManager.GetOrCreateFont(wndWindow.Font.Name, wndWindow.Font.Size, wndWindow.Font.Bold ? FontWeight.Bold : FontWeight.Normal);
            }

            result.TextColor = wndWindow.TextColor.Enabled.ToColorRgbaF();

            result.Text = wndWindow.Text.Translate();

            // TODO: TextBorderColor

            foreach (var childWindow in wndWindow.ChildWindows)
            {
                var child = CreateRecursive(childWindow, contentManager, wndCallbackResolver, wndRectangle.Location);
                result.Controls.Add(child);
            }

            return result;
        }

        private static Control CreateControl(WndWindowDefinition wndWindow, WndImageLoader imageLoader)
        {
            switch (wndWindow.WindowType)
            {
                case WndWindowType.CheckBox:
                    return new CheckBox(wndWindow, imageLoader);

                case WndWindowType.ComboBox:
                    return new ComboBox(wndWindow, imageLoader);

                case WndWindowType.HorizontalSlider:
                    return new Slider(wndWindow, imageLoader);

                case WndWindowType.ListBox:
                    return new ListBox(wndWindow, imageLoader);

                case WndWindowType.PushButton:
                    return new Button(wndWindow, imageLoader);

                case WndWindowType.RadioButton:
                    return new RadioButton(wndWindow, imageLoader);

                case WndWindowType.StaticText:
                    return new Label(wndWindow);

                case WndWindowType.EntryField:
                    return new TextBox(wndWindow, imageLoader);

                default: // TODO: Implement other window types.
                    var control = new Control();
                    if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
                    {
                        control.BackgroundImage = imageLoader.CreateNormalImage(wndWindow.EnabledDrawData, 0);
                    }
                    else
                    {
                        control.BackgroundColor = wndWindow.EnabledDrawData.Items[0].Color.ToColorRgbaF();
                    }
                    if (control.BackgroundImage == null)
                    {
                        control.BorderColor = wndWindow.EnabledDrawData.Items[0].BorderColor.ToColorRgbaF();
                        control.BorderWidth = 1;
                    }
                    return control;
            }
        }
    }
}
