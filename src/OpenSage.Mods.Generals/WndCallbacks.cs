using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Rep;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using OpenSage.Network;
using SixLabors.Fonts;
using Veldrid;

namespace OpenSage.Mods.Generals
{
    internal static class WndCallbacks
    {
        private static bool _doneMainMenuFadeIn;

        public static void W3DMainMenuInit(Window window, Game game)
        {
            if (!game.Configuration.LoadShellMap)
            {
                // Draw the main menu background if no map is loaded.
                window.Root.DrawCallback = window.Root.DefaultDraw;
            }

            // We'll show these later via window transitions.
            window.Controls.FindControl("MainMenu.wnd:MainMenuRuler").Hide();
            window.Controls.FindControl("MainMenu.wnd:MainMenuRuler").Opacity = 0;

            var initiallyHiddenSections = new[]
            {
                "MainMenu.wnd:MapBorder",
                "MainMenu.wnd:MapBorder1",
                "MainMenu.wnd:MapBorder2",
                "MainMenu.wnd:MapBorder3",
                "MainMenu.wnd:MapBorder4"
            };

            foreach (var controlName in initiallyHiddenSections)
            {
                var control = window.Controls.FindControl(controlName);
                control.Opacity = 0;

                foreach (var button in control.Controls.First().Controls)
                {
                    button.Opacity = 0;
                    button.TextOpacity = 0;
                }
            }

            window.Controls.FindControl("MainMenu.wnd:ButtonUSARecentSave").Hide();
            window.Controls.FindControl("MainMenu.wnd:ButtonUSALoadGame").Hide();

            window.Controls.FindControl("MainMenu.wnd:ButtonGLARecentSave").Hide();
            window.Controls.FindControl("MainMenu.wnd:ButtonGLALoadGame").Hide();

            window.Controls.FindControl("MainMenu.wnd:ButtonChinaRecentSave").Hide();
            window.Controls.FindControl("MainMenu.wnd:ButtonChinaLoadGame").Hide();

            // TODO: Show faction icons when WinScaleUpTransition is implemented.

            _doneMainMenuFadeIn = false;
        }

        public static void W3DNoDraw(Control control, DrawingContext2D drawingContext) { }

        public static void MainMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            void QueueTransition(string transition)
            {
                context.WindowManager.TransitionManager.QueueTransition(null, control.Window, transition);
            }

            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "MainMenu.wnd:ButtonSinglePlayer":
                            QueueTransition("MainMenuDefaultMenuBack");
                            QueueTransition("MainMenuSinglePlayerMenu");
                            break;

                        case "MainMenu.wnd:ButtonSkirmish":
                            context.WindowManager.SetWindow(@"Menus\SkirmishGameOptionsMenu.wnd");
                            break;

                        case "MainMenu.wnd:ButtonSingleBack":
                            QueueTransition("MainMenuSinglePlayerMenuBack");
                            QueueTransition("MainMenuDefaultMenu");
                            break;

                        case "MainMenu.wnd:ButtonMultiplayer":
                            QueueTransition("MainMenuDefaultMenuBack");
                            QueueTransition("MainMenuMultiPlayerMenu");
                            break;

                        case "MainMenu.wnd:ButtonMultiBack":
                            QueueTransition("MainMenuMultiPlayerMenuReverse");
                            QueueTransition("MainMenuDefaultMenu");
                            break;

                        case "MainMenu.wnd:ButtonLoadReplay":
                            QueueTransition("MainMenuDefaultMenuBack");
                            QueueTransition("MainMenuLoadReplayMenu");
                            break;

                        case "MainMenu.wnd:ButtonLoadReplayBack":
                            QueueTransition("MainMenuLoadReplayMenuBack");
                            QueueTransition("MainMenuDefaultMenu");
                            break;

                        case "MainMenu.wnd:ButtonReplay":
                            context.WindowManager.SetWindow(@"Menus\ReplayMenu.wnd");
                            break;

                        case "MainMenu.wnd:ButtonOptions":
                            context.WindowManager.PushWindow(@"Menus\OptionsMenu.wnd");
                            break;

                        case "MainMenu.wnd:ButtonCredits":
                            context.WindowManager.SetWindow(@"Menus\CreditsMenu.wnd");
                            break;

                        case "MainMenu.wnd:ButtonExit":
                            var exitWindow = context.WindowManager.PushWindow(@"Menus\QuitMessageBox.wnd");
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextTitle").Text = "EXIT?";
                            ((Label) exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextTitle")).TextAlignment = TextAlignment.Leading;
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextMessage").Text = "Are you sure you want to exit?";
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonOk").Show();
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonOk").Text = "YES";
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonCancel").Show();
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonCancel").Text = "NO";
                            break;
                    }
                    break;
            }
        }

        public static void MainMenuInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            // Any input at all (mouse, keyboard) will trigger the main menu fade-in.
            if (!_doneMainMenuFadeIn)
            {
                context.WindowManager.TransitionManager.QueueTransition(null, control.Window, "MainMenuFade");
                context.WindowManager.TransitionManager.QueueTransition(null, control.Window, "MainMenuDefaultMenu");
                control.Window.Controls.FindControl("MainMenu.wnd:MainMenuRuler").Show();
                _doneMainMenuFadeIn = true;
            }
        }

        public static void QuitMessageBoxSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "QuitMessageBox.wnd:ButtonCancel":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }

        public static void OptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "OptionsMenu.wnd:ButtonBack":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }

        private static FileSystem GetReplaysFileSystem(Game game) => new FileSystem(Path.Combine(game.UserDataFolder, "Replays"));

        public static void ReplayMenuInit(Window window, Game game)
        {
            var listBox = (ListBox) window.Controls.FindControl("ReplayMenu.wnd:ListboxReplayFiles");

            using (var fileSystem = GetReplaysFileSystem(game))
            {
                var newItems = new List<ListBoxDataItem>();

                foreach (var file in fileSystem.Files)
                {
                    var replayFile = ReplayFile.FromFileSystemEntry(file, onlyHeader: true);

                    newItems.Add(new ListBoxDataItem(
                        file.FilePath,
                        new[]
                        {
                            replayFile.Header.Filename, // Path.GetFileNameWithoutExtension(file.FilePath),
                            $"{replayFile.Header.Timestamp.Hour.ToString("D2")}:{replayFile.Header.Timestamp.Minute.ToString("D2")}",
                            replayFile.Header.Version,
                            replayFile.Header.Metadata.MapFile.Replace("maps/", string.Empty)
                        }));
                }

                listBox.Items = newItems.ToArray();
            }
        }

        public static void ReplayMenuShutdown(Window window, Game game)
        {
            // TODO
        }

        public static void ReplayMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "ReplayMenu.wnd:ButtonLoadReplay":
                            // TODO: Handle no selected item.

                            var listBox = (ListBox) control.Window.Controls.FindControl("ReplayMenu.wnd:ListboxReplayFiles");
                            ReplayFile replayFile;
                            using (var fileSystem = GetReplaysFileSystem(context.Game))
                            {
                                var replayFileEntry = fileSystem.GetFile((string) listBox.Items[listBox.SelectedIndex].DataItem);
                                replayFile = ReplayFile.FromFileSystemEntry(replayFileEntry);
                            }

                            // TODO: This probably isn't right.
                            var mapFilenameParts = replayFile.Header.Metadata.MapFile.Split('/');
                            var mapFilename = $"Maps\\{mapFilenameParts[1]}\\{mapFilenameParts[1]}.map";

                            context.Game.Scene2D.WndWindowManager.PopWindow();

                            context.Game.StartGame(mapFilename, new ReplayConnection(replayFile));

                            break;

                        case "ReplayMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Replay sub-menu
                            break;
                    }
                    break;
            }
        }

        public static void SkirmishGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishGameOptionsMenu.wnd:ButtonSelectMap":
                            context.WindowManager.PushWindow(@"Menus\SkirmishMapSelectMenu.wnd");
                            break;

                        case "SkirmishGameOptionsMenu.wnd:ButtonStart":
                            context.Game.Scene2D.WndWindowManager.PopWindow();
                            context.Game.StartGame(
                                @"maps\Alpine Assault\Alpine Assault.map", // TODO
                                new EchoConnection());
                            break;

                        case "SkirmishGameOptionsMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Single Player sub-menu
                            break;
                    }
                    break;
            }
        }

        public static void SkirmishMapSelectMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishMapSelectMenu.wnd:ButtonBack":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }

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
            switch (message.MessageType)
            {
                //case WndWindowMessageType.KeyDown:
                //    if (message.Key == Key.Escape)
                //    {
                //        context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                //    }
                //    break;
            }
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

        public static void PassSelectedButtonsToParentSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            if (message.MessageType != WndWindowMessageType.SelectedButton)
            {
                return;
            }

            control.Parent.SystemCallback.Invoke(control.Parent, message, context);
        }

        public static void PassMessagesToParentSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            control.Parent.SystemCallback.Invoke(control.Parent, message, context);
        }
    }
}
