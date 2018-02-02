using OpenSage.Data;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd;

namespace OpenSage.Content
{
    internal sealed class WindowLoader : ContentLoader<WndTopLevelWindow>
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

        protected override WndTopLevelWindow LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            var wndFile = WndFile.FromFileSystemEntry(entry);

            var result = CreateElementRecursive(
                wndFile.RootWindow,
                contentManager,
                _wndCallbackResolver);

            var window = new WndTopLevelWindow(wndFile, result, _wndCallbackResolver);

            void setWindowRecursive(WndWindow element)
            {
                element.Window = window;

                foreach (var child in element.Children)
                {
                    setWindowRecursive(child);
                }
            }

            setWindowRecursive(result);

            return window;
        }

        private static WndWindow CreateElementRecursive(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver wndCallbackResolver)
        {
            var result = new WndWindow(wndWindow, contentManager, wndCallbackResolver);

            foreach (var childWindow in wndWindow.ChildWindows)
            {
                var child = CreateElementRecursive(childWindow, contentManager, wndCallbackResolver);
                child.Parent = result;
                result.Children.Add(child);
            }

            return result;
        }
    }
}
