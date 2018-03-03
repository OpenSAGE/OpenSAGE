using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;

namespace OpenSage.Mods.Bfme2
{
    public class Bfme2RotwkDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Bfme2Rotwk;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm) II: The Rise of the Witch-king";
        public IGameDefinition BaseGame => Bfme2Definition.Instance;

        // TODO: Localise?
        public string LauncherImagePath => "EnglishSplash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Lord of the Rings, The Rise of the Witch-king", "InstallPath") 
        };

        public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");

        public static Bfme2RotwkDefinition Instance { get; } = new Bfme2RotwkDefinition();
    }
}
