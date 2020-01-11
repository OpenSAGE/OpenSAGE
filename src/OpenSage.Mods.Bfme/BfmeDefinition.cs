using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;

namespace OpenSage.Mods.BFME
{
    public class BfmeDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Bfme;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm)";
        public IGameDefinition BaseGame => null;

        public bool LauncherImagePrefixLang => true;
        public string LauncherImagePath => "Splash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\The Battle for Middle-earth", "InstallPath")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
{
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\The Battle for Middle-earth", "Language")
        };

        public string Identifier { get; } = "bfme";

        public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");
        public IControlBarSource ControlBar { get; }

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
        {
            return new OnDemandAssetLoadStrategy(PathResolvers.W3d, PathResolvers.BfmeTexture);
        }

        public static BfmeDefinition Instance { get; } = new BfmeDefinition();
    }
}
