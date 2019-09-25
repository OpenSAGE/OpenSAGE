using OpenSage.Content.Translation;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class ContentManager : DisposableBase
    {
        private readonly Game _game;

        public ISubsystemLoader SubsystemLoader { get; }

        private readonly FileSystem _fileSystem;

        public GraphicsDevice GraphicsDevice { get; }

        public SageGame SageGame { get; }

        public FileSystem FileSystem => _fileSystem;

        public IniDataContext IniDataContext { get; }

        public ITranslationManager TranslationManager { get; }

        public WndImageLoader WndImageLoader { get; }

        public FontManager FontManager { get; }

        public string Language { get; }

        public ContentManager(
            Game game,
            FileSystem fileSystem,
            GraphicsDevice graphicsDevice,
            SageGame sageGame)
        {
            using (GameTrace.TraceDurationEvent("ContentManager()"))
            {
                _game = game;
                _fileSystem = fileSystem;

                GraphicsDevice = graphicsDevice;

                SageGame = sageGame;

                Language = LanguageUtility.ReadCurrentLanguage(game.Definition, fileSystem.RootDirectory);

                IniDataContext = new IniDataContext();

                SubsystemLoader = Content.SubsystemLoader.Create(game.Definition, _fileSystem, game, this);

                switch (sageGame)
                {
                    // Only load these INI files for a subset of games, because we can't parse them for others yet.
                    case SageGame.CncGenerals:
                    case SageGame.CncGeneralsZeroHour:
                    case SageGame.Bfme:
                    case SageGame.Bfme2:
                    case SageGame.Bfme2Rotwk:
                        SubsystemLoader.Load(Subsystem.Core);

                        // TODO: Defer subsystem loading until necessary
                        SubsystemLoader.Load(Subsystem.Audio);
                        SubsystemLoader.Load(Subsystem.Players);
                        SubsystemLoader.Load(Subsystem.ParticleSystems);
                        SubsystemLoader.Load(Subsystem.ObjectCreation);
                        SubsystemLoader.Load(Subsystem.Multiplayer);
                        SubsystemLoader.Load(Subsystem.LinearCampaign);
                        SubsystemLoader.Load(Subsystem.Wnd);
                        SubsystemLoader.Load(Subsystem.Terrain);
                        SubsystemLoader.Load(Subsystem.Credits);

                        break;

                    case SageGame.Cnc3:
                        SubsystemLoader.Load(Subsystem.Core);
                        break;

                    default:
                        break;
                }

                TranslationManager = Translation.TranslationManager.Instance;
                Translation.TranslationManager.LoadGameStrings(fileSystem, Language, sageGame);

                FontManager = new FontManager();

                WndImageLoader = AddDisposable(new WndImageLoader(GraphicsDevice, game.AssetStore));
            }
        }

        // TODO: Move these methods to somewhere else (SubsystemLoader?)
        internal void LoadIniFiles(string folder)
        {
            foreach (var iniFile in _fileSystem.GetFiles(folder))
            {
                LoadIniFile(iniFile);
            }
        }

        internal void LoadIniFile(string filePath)
        {
            LoadIniFile(_fileSystem.GetFile(filePath));
        }

        internal void LoadIniFile(FileSystemEntry entry)
        {
            using (GameTrace.TraceDurationEvent($"LoadIniFile('{entry.FilePath}'"))
            {
                if (!entry.FilePath.ToLowerInvariant().EndsWith(".ini"))
                {
                    return;
                }

                var parser = new IniParser(entry, _game.AssetStore, _game.SageGame, IniDataContext);
                parser.ParseFile();
            }
        }
    }
}
