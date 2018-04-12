using System;
using System.IO;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.Data.StreamFS;
using OpenSage.DataViewer.UI.Viewers;

namespace OpenSage.DataViewer.UI
{
    public sealed class ContentView : Panel
    {
        private readonly Func<GameInstallation> _getInstallation;
        private readonly Func<FileSystem> _getFileSystem;

        private GameWindow _gameWindow;
        private Game _game;

        public ContentView(Func<GameInstallation> getInstallation, Func<FileSystem> getFileSystem)
        {
            _getInstallation = getInstallation;
            _getFileSystem = getFileSystem;
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
            Game getGame(IntPtr windowHandle)
            {
                if (_gameWindow == null)
                {
                    _gameWindow = new GameWindow(windowHandle);
                }

                _game = GameFactory.CreateGame(
                    _getInstallation(),
                    _getFileSystem(),
                    GamePanel.FromGameWindow(_gameWindow));

                return _game;
            }

            switch (Path.GetExtension(entry.FilePath).ToLower())
            {
                case ".ani":
                case ".cur":
                    return new AniView(entry, getGame);

                case ".apt":
                    return new AptView(entry, getGame);

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
                    return new IniView(entry, getGame);

                case ".manifest":
                    return new ManifestView(entry, _getInstallation, _getFileSystem);

                case ".map":
                    return new MapView(entry, getGame);

                case ".ru":
                    return new RuView(entry, getGame);

                case ".tga":
                    return new TgaView(entry);

                case ".txt":
                    return new TxtView(entry);

                case ".wav":
                    return new WavView(entry, getGame);

                case ".w3d":
                    return new W3dView(entry, getGame);

                case ".wnd":
                    return new WndView(entry, getGame);

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

            _game?.Dispose();
            _game = null;

            _gameWindow?.Dispose();
            _gameWindow = null;

            base.Dispose(disposing);
        }
    }
}
