using OpenSage.Content.Translation;
using OpenSage.Data.Ini;
using OpenSage.Data.IO;
using OpenSage.Diagnostics;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class ContentManager : DisposableBase
    {
        private readonly Game _game;

        public ISubsystemLoader SubsystemLoader { get; }

        public GraphicsDevice GraphicsDevice { get; }

        public SageGame SageGame { get; }

        public IniDataContext IniDataContext { get; }

        public ITranslationManager TranslationManager { get; }

        public FontManager FontManager { get; }

        public string Language { get; }

        public ContentManager(
            Game game,
            GraphicsDevice graphicsDevice,
            SageGame sageGame)
        {
            using (GameTrace.TraceDurationEvent("ContentManager()"))
            {
                _game = game;

                GraphicsDevice = graphicsDevice;

                SageGame = sageGame;

                Language = LanguageUtility.ReadCurrentLanguage(game.Definition, "/game/");

                IniDataContext = new IniDataContext();

                SubsystemLoader = Content.SubsystemLoader.Create(game.Definition, game, this);

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
                Translation.TranslationManager.LoadGameStrings(Language, sageGame);

                FontManager = new FontManager();
            }
        }

        // TODO: Move these methods to somewhere else (SubsystemLoader?)
        internal void LoadIniFiles(string folder)
        {
            foreach (var iniFile in FileSystem.ListFiles(folder, "*", SearchOption.AllDirectories))
            {
                LoadIniFile(iniFile);
            }
        }

        internal void LoadIniFile(string url)
        {
            using (GameTrace.TraceDurationEvent($"LoadIniFile('{url}'"))
            {
                if (!url.ToLowerInvariant().EndsWith(".ini"))
                {
                    return;
                }

                var parser = new IniParser(url, _game.AssetStore, _game.SageGame, IniDataContext);
                parser.ParseFile();
            }
        }
    }
}
