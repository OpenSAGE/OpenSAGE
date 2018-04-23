using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using SixLabors.Fonts;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class CreditsMenuCallbacks
    {
        private sealed class CreditsMenuData
        {
            public readonly List<CreditsMenuDataItem> Items = new List<CreditsMenuDataItem>();

            public float CurrentY;
        }

        private sealed class CreditsMenuDataItem
        {
            public string Text;
            public Font Font;
            public ColorRgba Color;
            public RectangleF Rect;
        }

        public static void CreditsMenuInit(Window window, Game game)
        {
            game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\Credits.ini");
            var credits = game.ContentManager.IniDataContext.Credits;

            var control = window.Controls.FindControl("CreditsMenu.wnd:WinTextDraw");

            var data = new CreditsMenuData
            {
                CurrentY = control.Height
            };

            var fontSize = 14;
            var color = credits.NormalColor;
            var y = 0f;

            foreach (var line in credits.Lines)
            {
                string text = null;
                string measureText = null;
                switch (line)
                {
                    case CreditStyleLine sl:
                        switch (sl.Style)
                        {
                            case CreditStyle.Title:
                                fontSize = 30;
                                color = credits.TitleColor;
                                break;
                            case CreditStyle.MinorTitle:
                                fontSize = 22;
                                color = credits.MinorTitleColor;
                                break;
                            case CreditStyle.Normal:
                                color = credits.NormalColor;
                                fontSize = 14;
                                break;
                            case CreditStyle.Column:
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                        continue;

                    case CreditTextLine tl:
                        text = measureText = tl.Text.Contains(":")
                            ? game.ContentManager.TranslationManager.Lookup(tl.Text)
                            : tl.Text;
                        break;

                    case CreditBlankLine bl:
                        text = " ";
                        measureText = "a";
                        break;
                }

                var font = game.ContentManager.GetOrCreateFont("Arial", fontSize, FontWeight.Normal);
                var height = DrawingContext2D.MeasureText(measureText, font, TextAlignment.Center, control.Width).Height;

                data.Items.Add(new CreditsMenuDataItem
                {
                    Text = text,
                    Font = font,
                    Color = color,
                    Rect = new RectangleF(0, y, control.Width, height)
                });

                y += height;
            }

            control.Tag = data;
        }

        public static void CreditsMenuUpdate(Window window, Game game)
        {
            var control = window.Controls.FindControl("CreditsMenu.wnd:WinTextDraw");
            var data = (CreditsMenuData) control.Tag;

            var credits = game.ContentManager.IniDataContext.Credits;

            var multiplier = credits.ScrollDown ? 1 : -1;

            data.CurrentY += credits.ScrollRate * multiplier;

            // TODO: credits.ScrollRateEveryFrames

            // TODO: Go back to main menu after credits finish.
        }

        public static void CreditsMenuInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            //switch (message.MessageType)
            //{
            //    case WndWindowMessageType.KeyDown:
            //        if (message.Key == Key.Escape)
            //        {
            //            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
            //        }
            //        break;
            //}
        }

        public static void W3DCreditsMenuDraw(Control control, DrawingContext2D drawingContext)
        {
            var data = (CreditsMenuData) control.Tag;

            foreach (var item in data.Items)
            {
                var rect = item.Rect;
                rect.Y += data.CurrentY;

                if (!control.ClientRectangle.ToRectangleF().IntersectsWith(rect))
                {
                    continue;
                }

                drawingContext.DrawText(
                    item.Text,
                    item.Font,
                    TextAlignment.Center,
                    item.Color.ToColorRgbaF(),
                    rect);
            }
        }
    }
}
