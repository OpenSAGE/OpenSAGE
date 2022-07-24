using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.CommandListOverlay;
using OpenSage.Mods.Bfme.Gui;
using OpenSage.Mods.Bfme;
using System.IO;
using OpenSage.Core.Graphics;
using OpenSage.Core.Graphics.W3d;

namespace OpenSage.Mods.Bfme2
{
    public class Bfme2Definition : IGameDefinition
    {
        public SageGame Game => SageGame.Bfme2;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm) II";
        public IGameDefinition BaseGame => null;

        public bool LauncherImagePrefixLang => true;
        public string LauncherImagePath => "Splash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "InstallPath")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "Language")
        };

        public string Identifier { get; } = "bfme2";

        public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");
        public IControlBarSource ControlBar { get; } = new AptControlBarSource();
        public ICommandListOverlaySource CommandListOverlay { get; } = new RadialUnitOverlaySource();

        public uint ScriptingTicksPerSecond => 5;

        public string GetLocalizedStringsPath(string language) => language == "German"
            ? "lotr"
            : Path.Combine("data", "lotr");

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
        {
            return new OnDemandAssetLoadStrategy(W3dPathResolvers.Bfme2, TexturePathResolvers.Bfme2, PathResolvers.Bfme2Texture);
        }

        public static Bfme2Definition Instance { get; } = new Bfme2Definition();

        public string LauncherExecutable => "lotrbfme2.exe";
    }
}
