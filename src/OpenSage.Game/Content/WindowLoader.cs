using System;
using OpenSage.Data;
using OpenSage.Data.Wnd;
using OpenSage.Gui;
using OpenSage.Gui.Elements;

namespace OpenSage.Content
{
    internal sealed class WindowLoader : ContentLoader<GuiWindow>
    {
        public WindowLoader(ContentManager contentManager)
        {
            switch (contentManager.SageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    contentManager.IniDataContext.LoadIniFile(@"Data\English\HeaderTemplate.ini");
                    break;

                case SageGame.BattleForMiddleEarth:
                case SageGame.BattleForMiddleEarthII:
                    contentManager.IniDataContext.LoadIniFile(@"lang\english\headertemplate.ini");
                    break;
            }

            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\HandCreated\");
            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\TextureSize_512\");
        }

        protected override GuiWindow LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            var wndFile = WndFile.FromFileSystemEntry(entry);

            var result = CreateElementRecursive(
                wndFile.RootWindow, 
                contentManager);

            return new GuiWindow(wndFile, result);
        }

        private static UIElement CreateElementRecursive(WndWindow wndWindow, ContentManager contentManager)
        {
            UIElement createElement()
            {
                switch (wndWindow.WindowType)
                {
                    case WndWindowType.GenericWindow:
                        return new GenericWindow();

                    case WndWindowType.PushButton:
                        return new PushButton();

                    case WndWindowType.StaticText:
                        return new StaticText();

                    case WndWindowType.EntryField:
                        return new EntryField();

                    case WndWindowType.CheckBox:
                        return new CheckBox();

                    case WndWindowType.ComboBox:
                        return new ComboBox();

                    case WndWindowType.HorizontalSlider:
                        return new HorizontalSlider();

                    case WndWindowType.VerticalSlider:
                        return new VerticalSlider();

                    case WndWindowType.ListBox:
                        return new ListBox();

                    case WndWindowType.ProgressBar:
                        return new ProgressBar();

                    case WndWindowType.RadioButton:
                        return new RadioButton();

                    default:
                        throw new NotSupportedException();
                }
            }

            var result = createElement();

            result.Initialize(wndWindow, contentManager);

            foreach (var childWindow in wndWindow.ChildWindows)
            {
                var child = CreateElementRecursive(childWindow, contentManager);
                child.Parent = result;
                result.Children.Add(child);
            }

            return result;
        }
    }
}
