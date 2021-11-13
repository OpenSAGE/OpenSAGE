using System;
using System.Text;
using OpenSage.Content.Translation;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics;
using OpenSage.IO;
using OpenSage.Logic.Object;
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

        public FileSystem FileSystem { get; }
        public DiskFileSystem UserDataFileSystem { get; internal set; }

        public IniDataContext IniDataContext { get; }

        public ITranslationManager TranslationManager { get; }
        // LocaleSpecificEncoding is Mainly used by "9x" ini files
        public Encoding LocaleSpecificEncoding { get; private set; }

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

                FileSystem = fileSystem;

                GraphicsDevice = graphicsDevice;

                SageGame = sageGame;

                Language = LanguageUtility.ReadCurrentLanguage(game.Definition, fileSystem);

                TranslationManager = Translation.TranslationManager.Instance;
                Translation.TranslationManager.LoadGameStrings(fileSystem, Language, game.Definition);
                LocaleSpecificEncoding = Encoding.GetEncoding(TranslationManager.CurrentLanguage.TextInfo.ANSICodePage);

                void OnLanguageChanged(object sender, EventArgs e)
                {
                    throw new NotImplementedException("Encoding change on LanguageChanged not implemented yet");
                }

                TranslationManager.LanguageChanged += OnLanguageChanged;
                AddDisposeAction(() => TranslationManager.LanguageChanged -= OnLanguageChanged);

                IniDataContext = new IniDataContext();

                SubsystemLoader = Content.SubsystemLoader.Create(game.Definition, FileSystem, game, this);

                switch (sageGame)
                {
                    // Only load these INI files for a subset of games, because we can't parse them for others yet.
                    // TODO: Defer subsystem loading until necessary
                    case SageGame.CncGenerals:
                    case SageGame.CncGeneralsZeroHour:
                        SubsystemLoader.Load(Subsystem.Core);
                        SubsystemLoader.Load(Subsystem.Audio);
                        SubsystemLoader.Load(Subsystem.Players);
                        SubsystemLoader.Load(Subsystem.ParticleSystems);
                        SubsystemLoader.Load(Subsystem.ObjectCreation);
                        SubsystemLoader.Load(Subsystem.Locomotors);
                        SubsystemLoader.Load(Subsystem.Sciences);
                        SubsystemLoader.Load(Subsystem.Weapons);
                        SubsystemLoader.Load(Subsystem.FXList);
                        SubsystemLoader.Load(Subsystem.Multiplayer);
                        SubsystemLoader.Load(Subsystem.LinearCampaign);
                        SubsystemLoader.Load(Subsystem.Wnd);
                        SubsystemLoader.Load(Subsystem.Terrain);
                        SubsystemLoader.Load(Subsystem.Credits);
                        SubsystemLoader.Load(Subsystem.Damage);
                        SubsystemLoader.Load(Subsystem.SpecialPower);
                        SubsystemLoader.Load(Subsystem.InGameUI);
                        SubsystemLoader.Load(Subsystem.Rank);
                        break;

                    case SageGame.Bfme:
                    case SageGame.Bfme2:
                    case SageGame.Bfme2Rotwk:
                        SubsystemLoader.Load(Subsystem.Core);
                        SubsystemLoader.Load(Subsystem.Audio);
                        SubsystemLoader.Load(Subsystem.Players);
                        SubsystemLoader.Load(Subsystem.ParticleSystems);
                        SubsystemLoader.Load(Subsystem.ObjectCreation);
                        SubsystemLoader.Load(Subsystem.Locomotors);
                        SubsystemLoader.Load(Subsystem.Sciences);
                        SubsystemLoader.Load(Subsystem.Weapons);
                        SubsystemLoader.Load(Subsystem.FXList);
                        SubsystemLoader.Load(Subsystem.Multiplayer);
                        SubsystemLoader.Load(Subsystem.LinearCampaign);
                        SubsystemLoader.Load(Subsystem.Wnd);
                        SubsystemLoader.Load(Subsystem.Terrain);
                        SubsystemLoader.Load(Subsystem.Credits);
                        SubsystemLoader.Load(Subsystem.Damage);
                        SubsystemLoader.Load(Subsystem.SpecialPower);
                        SubsystemLoader.Load(Subsystem.InGameUI);
                        SubsystemLoader.Load(Subsystem.Rank);

                        SubsystemLoader.Load(Subsystem.ExperienceLevels);
                        SubsystemLoader.Load(Subsystem.AttributeModifiers);
                        break;

                    case SageGame.Cnc3:
                    case SageGame.Ra3:
                        SubsystemLoader.Load(Subsystem.Core);
                        break;

                    default:
                        break;
                }

                UpgradeManager.Initialize(_game.AssetStore);

                FontManager = new FontManager(Language, StringComparer.Create(TranslationManager.CurrentLanguage, true));
            }
        }

        // TODO: Move these methods to somewhere else (SubsystemLoader?)
        internal void LoadIniFiles(string folder)
        {
            foreach (var iniFile in FileSystem.GetFilesInDirectory(folder, "*.ini"))
            {
                LoadIniFile(iniFile);
            }
        }

        internal void LoadIniFile(string filePath)
        {
            LoadIniFile(FileSystem.GetFile(filePath));
        }

        internal void LoadIniFile(FileSystemEntry entry)
        {
            using (GameTrace.TraceDurationEvent($"LoadIniFile('{entry.FilePath}'"))
            {
                var parser = new IniParser(entry, _game.AssetStore, _game.SageGame, IniDataContext, LocaleSpecificEncoding);
                parser.ParseFile();
            }
        }

        internal FileSystemEntry GetMapEntry(string mapPath)
        {
            if (UserDataFileSystem is not null)
            {
                var mapEntry = UserDataFileSystem.GetFile(mapPath);
                if (mapEntry is not null)
                {
                    return mapEntry;
                }
            }

            return FileSystem.GetFile(mapPath);
        }

        internal FileSystemEntry GetScriptEntry(string scriptPath) => FileSystem.GetFile(scriptPath);
    }
}
