using System;
using System.Text;
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

        public GraphicsDevice GraphicsDevice { get; }

        public SageGame SageGame { get; }

        public FileSystem FileSystem { get; }
        public FileSystem UserDataFileSystem { get; internal set; }

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

                Language = LanguageUtility.ReadCurrentLanguage(game.Definition, fileSystem.RootDirectory);

                TranslationManager = Translation.TranslationManager.Instance;
                Translation.TranslationManager.LoadGameStrings(fileSystem, Language, sageGame);
                LocaleSpecificEncoding = Encoding.GetEncoding(TranslationManager.CurrentLanguage.TextInfo.ANSICodePage);
                TranslationManager.LanguageChanged +=
                    (sender, e) => throw new NotImplementedException("Encoding change on LanguageChanged not implemented yet");

                IniDataContext = new IniDataContext();

                SubsystemLoader = Content.SubsystemLoader.Create(game.Definition, FileSystem, game, this);

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
                        SubsystemLoader.Load(Subsystem.Locomotors);
                        SubsystemLoader.Load(Subsystem.Weapons);
                        SubsystemLoader.Load(Subsystem.FXList);
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

                FontManager = new FontManager(Language, StringComparer.Create(TranslationManager.CurrentLanguage, true));
            }
        }

        // TODO: Move these methods to somewhere else (SubsystemLoader?)
        internal void LoadIniFiles(string folder)
        {
            foreach (var iniFile in FileSystem.GetFiles(folder))
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
                if (!entry.FilePath.ToLowerInvariant().EndsWith(".ini"))
                {
                    return;
                }

                var parser = new IniParser(entry, _game.AssetStore, _game.SageGame, IniDataContext, LocaleSpecificEncoding);
                parser.ParseFile();
            }
        }

        internal FileSystemEntry GetMapEntry(string mapPath)
        {
            var normalizedPath = FileSystem.NormalizeFilePath(mapPath);
            if (UserDataFileSystem != null && normalizedPath.StartsWith(UserDataFileSystem.RootDirectory))
            {
                mapPath = mapPath.Substring(UserDataFileSystem.RootDirectory.Length + 1);
                return UserDataFileSystem.GetFile(mapPath);
            }
            else
            {
                return FileSystem.GetFile(mapPath);
            }
        }
    }
}
