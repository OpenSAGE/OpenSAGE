using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.CommandListOverlay;
using OpenSage.Gui.ControlBar;
using OpenSage.Mathematics;

namespace OpenSage.Tools.AptEditor
{
    internal class AptEditorDefinition : IGameDefinition
    {
        public static IPathResolver TexturePathResolver => PathResolvers.Bfme2Texture;
        public SageGame Game => SageGame.Tool;
        public string DisplayName => "Apt Editor";
        public IGameDefinition? BaseGame => null;
        public bool LauncherImagePrefixLang => false;
        public string? LauncherImagePath => null;
        public IEnumerable<RegistryKeyPath> RegistryKeys => Enumerable.Empty<RegistryKeyPath>();
        public IEnumerable<RegistryKeyPath>? LanguageRegistryKeys => null;
        public IMainMenuSource? MainMenu => new AptEditorBackgroundSource();
        public IControlBarSource? ControlBar => null;
        public string Identifier => "AptEditor";
        public string LauncherExecutable => throw new NotSupportedException("This is apt editor");
        public ICommandListOverlaySource CommandListOverlay => throw new NotSupportedException("This is apt editor");
        public uint ScriptingTicksPerSecond => 1; // actually not used

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
        {
            return new OnDemandAssetLoadStrategy(PathResolvers.W3d, TexturePathResolver);
        }
    }

    internal class AptEditorBackgroundSource : IMainMenuSource
    {
        public void AddToScene(Game game, Scene2D scene, bool useShellMap)
        {
            scene.AptWindowManager.PushWindow(CreateBackgroundAptWindow(game, new ColorRgba(0, 96, 0, 255)));
        }

        public static AptWindow CreateBackgroundAptWindow(Game game, in ColorRgba color)
        {
            return new AptWindow(game, game.ContentManager, CreateBackgroundFile(color));
        }

        public static AptFile CreateBackgroundFile(in ColorRgba color)
        {
            const int width = 100;
            const int height = 100;
            var aptFile = AptFile.CreateEmpty("bg", width, height, 1000);
            var placeBackground = PlaceObject.Create(0, SampleApt.CreateShape(aptFile, new[]
            {
                "c",
                $"s s:{color.R}:{color.G}:{color.B}:{color.A}",
                $"t 0:0:{width}:{height}:0:{height}",
                $"t 0:0:{width}:{height}:{width}:0"
            }));
            var frame = Frame.Create(new List<FrameItem> { placeBackground });
            aptFile.Movie.Frames.Add(frame);

            return aptFile;
        }
    }
}
