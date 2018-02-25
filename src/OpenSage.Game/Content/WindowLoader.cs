using System.Linq;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.Wnd;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Content
{
    internal sealed class WindowLoader : ContentLoader<Window>
    {
        private readonly WndCallbackResolver _wndCallbackResolver;

        public WindowLoader(ContentManager contentManager, WndCallbackResolver wndCallbackResolver)
        {
            _wndCallbackResolver = wndCallbackResolver;

            switch (contentManager.SageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    contentManager.IniDataContext.LoadIniFile(@"Data\English\HeaderTemplate.ini");
                    break;

                case SageGame.BattleForMiddleEarth:
                    contentManager.IniDataContext.LoadIniFile(@"lang\english\headertemplate.ini");
                    break;

                case SageGame.BattleForMiddleEarthII:
                    contentManager.IniDataContext.LoadIniFile(@"headertemplate.ini");
                    break;
            }

            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\HandCreated\");
            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\TextureSize_512\");
        }

        protected override Window LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            var wndFile = WndFile.FromFileSystemEntry(entry);

            var rootControl = CreateElementRecursive(
                wndFile.RootWindow,
                contentManager,
                _wndCallbackResolver,
                wndFile.RootWindow.ScreenRect.ToRectangle().Location);

            return new Window(wndFile.RootWindow.ScreenRect.CreationResolution, rootControl, contentManager)
            {
                LayoutInit = _wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutInit),
                LayoutUpdate = _wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutUpdate),
                LayoutShutdown = _wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutShutdown)
            };
        }

        private static Control CreateElementRecursive(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver wndCallbackResolver, Point2D parentOffset)
        {
            var imageLoader = contentManager.WndImageLoader;

            Control createControl()
            {
                switch (wndWindow.WindowType)
                {
                    case WndWindowType.ListBox:
                        return new ListBox();

                    case WndWindowType.PushButton:
                        return new Button
                        {
                            BackgroundImage = imageLoader.GetStretchableImage(wndWindow.EnabledDrawData, 0, 5, 6),
                            HoverBackgroundImage = imageLoader.GetStretchableImage(wndWindow.HiliteDrawData, 0, 5, 6),
                            DisabledBackgroundImage = imageLoader.GetStretchableImage(wndWindow.DisabledDrawData, 0, 5, 6),
                            PushedBackgroundImage = imageLoader.GetStretchableImage(wndWindow.HiliteDrawData, 1, 3, 4),

                            HoverTextColor = wndWindow.TextColor.Hilite.ToColorRgbaF(),
                            DisabledTextColor = wndWindow.TextColor.Disabled.ToColorRgbaF()
                        };

                    case WndWindowType.StaticText:
                        return new Label
                        {
                            TextAlignment = wndWindow.StaticTextData.Centered
                                ? TextAlignment.Center
                                : TextAlignment.Leading
                        };

                    default: // TODO: Implement other window types.
                        {
                            var control = new Control();
                            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
                            {
                                control.BackgroundImage = imageLoader.GetNormalImage(wndWindow.EnabledDrawData, 0);
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

            var result = createControl();

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

            if (wndWindow.HasHeaderTemplate)
            {
                var headerTemplate = contentManager.IniDataContext.HeaderTemplates.First(x => x.Name == wndWindow.HeaderTemplate);
                result.Font = new DrawingFont(headerTemplate.Font, headerTemplate.Point, headerTemplate.Bold);
            }
            else
            {
                result.Font = new DrawingFont(wndWindow.Font.Name, wndWindow.Font.Size, wndWindow.Font.Bold);
            }

            result.TextColor = wndWindow.TextColor.Enabled.ToColorRgbaF();

            result.Text = contentManager.TranslationManager.Lookup(wndWindow.Text);

            // TODO: TextBorderColor

            foreach (var childWindow in wndWindow.ChildWindows)
            {
                var child = CreateElementRecursive(childWindow, contentManager, wndCallbackResolver, wndRectangle.Location);
                result.Controls.Add(child);
            }

            return result;
        }
    }
}
