using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;

namespace OpenSage.Mods.Bfme2
{
    public class Bfme2RotwkDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Bfme2Rotwk;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm) II: The Rise of the Witch-king";
        public IGameDefinition BaseGame => Bfme2Definition.Instance;

        public bool LauncherImagePrefixLang => true;
        public string LauncherImagePath => "Splash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Lord of the Rings, The Rise of the Witch-king", "InstallPath")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "Language")
        };

        public string Identifier { get; } = "bfme2_rotwk";

        public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");
        public IControlBarSource ControlBar { get; }

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
        {
            return new OnDemandAssetLoadStrategy(PathResolvers.Bfme2W3d, PathResolvers.Bfme2Texture);
        }

        public static Bfme2RotwkDefinition Instance { get; } = new Bfme2RotwkDefinition();

        public string LauncherExecutable => "lotrbfme2ep1.exe";
    }
}
