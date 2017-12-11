using System;
using System.IO;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.UI.Viewers;

namespace OpenSage.DataViewer.UI
{
    public sealed class ContentView : Panel
    {
        private readonly Func<Game> _getGame;

        public ContentView(Func<Game> getGame)
        {
            _getGame = getGame;
        }

        public void SetContent(FileSystemEntry entry)
        {
            if (Content != null)
            {
                var existingContent = Content;
                Content = null;
                existingContent.Dispose();
            }

            if (entry != null)
            {
                Content = CreateControl(entry);
            }
        }

        private Control CreateControl(FileSystemEntry entry)
        {
            switch (Path.GetExtension(entry.FilePath).ToLower())
            {
                case ".ani":
                    return new AniView(entry);

                case ".bmp":
                    return new BmpView(entry);

                case ".dds":
                    return new DdsView(entry);

                case ".csf":
                    return new CsfView(entry);

                case ".ini":
                    return new IniView();

                case ".map":
                    return new MapView(entry, _getGame());

                case ".tga":
                    return new TgaView(entry);

                case ".txt":
                    return new TxtView(entry);

                case ".w3d":
                    return new W3dView(entry, _getGame());

                case ".wnd":
                    return new WndView(entry, _getGame());

                default:
                    return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            Content?.Dispose();

            base.Dispose(disposing);
        }
    }
}
