using System;
using System.IO;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.Data.StreamFS;
using OpenSage.Data.Tga;
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

        private void SetContent<TContent>(TContent content, Func<TContent, Control> createControl)
        {
            if (Content != null)
            {
                var existingContent = Content;
                Content = null;
                existingContent.Dispose();
            }

            if (content != null)
            {
                Content = createControl(content);
            }
        }

        public void SetContent(FileSystemEntry entry)
        {
            SetContent(entry, CreateControlForFileSystemEntry);
        }

        private Control CreateControlForFileSystemEntry(FileSystemEntry entry)
        {
            switch (Path.GetExtension(entry.FilePath).ToLower())
            {
                case ".ani":
                case ".cur":
                    return new AniView(entry, _getGame());

                case ".apt":
                    return new AptView(entry, _getGame());

                case ".bmp":
                    return new BmpView(entry);

                case ".dds":
                    if (!DdsFile.IsDdsFile(entry))
                    {
                        goto case ".tga";
                    }
                    return new DdsView(DdsFile.FromFileSystemEntry(entry));

                case ".const":
                    return new ConstView(entry);

                case ".csf":
                    return new CsfView(entry);

                case ".ini":
                    return new IniView(entry, _getGame());

                case ".manifest":
                    return new ManifestView(entry, _getGame);

                case ".map":
                    return new MapView(entry, _getGame());

                case ".ru":
                    return new RuView(entry, _getGame());

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

        public void SetContent(Asset asset)
        {
            SetContent(asset, CreateControlForAsset);
        }

        private Control CreateControlForAsset(Asset asset)
        {
            switch (asset.AssetType)
            {
                case AssetType.Texture:
                    return new DdsView((DdsFile) asset.InstanceData);

                default:
                    return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            var content = Content;
            Content = null;
            content?.Dispose();

            base.Dispose(disposing);
        }
    }
}
