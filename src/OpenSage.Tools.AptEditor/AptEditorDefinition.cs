using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.UnitOverlay;
using OpenSage.Mathematics;

namespace OpenSage.Tools.AptEditor
{
    internal class AptEditorDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Ra3;
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
        public IUnitOverlaySource UnitOverlay => throw new NotSupportedException("This is apt editor");
        public uint ScriptingTicksPerSecond => 1; // actually not used

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
        {
            return new OnDemandAssetLoadStrategy(PathResolvers.W3d, PathResolvers.Bfme2Texture);
        }
    }

    internal class AptEditorBackgroundSource : IMainMenuSource
    {
        public void AddToScene(Game game, Scene2D scene, bool useShellMap)
        {
            scene.AptWindowManager.PushWindow(CreateBackgroundAptWindow(game, new ColorRgba(0, 0, 255, 255)));
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

            using var geometryCommands = new StringReader(string.Join('\n', new[]
            {
                "c",
                $"s s:{color.R}:{color.G}:{color.B}:{color.A}",
                $"t 0:0:{width}:{height}:0:{height}",
                $"t 0:0:{width}:{height}:{width}:0"
            }));

            var geometry = 0u;
            aptFile.GeometryMap.Add(geometry, Geometry.Parse(aptFile, geometryCommands));
            var backgroundShape = Shape.Create(aptFile, new Vector4(0, 0, width, height), geometry);
            var placeBackground = PlaceObject.CreatePlace(0, backgroundShape);
            var frame = Frame.Create(new List<FrameItem> { placeBackground });
            aptFile.Movie.Frames.Add(frame);

            return aptFile;
        }
    }
}
