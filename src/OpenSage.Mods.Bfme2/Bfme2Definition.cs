using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;

namespace OpenSage.Mods.Bfme2
{
    public class Bfme2Definition : IGameDefinition
    {
        public SageGame Game => SageGame.Bfme2;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm) II";
        public IGameDefinition BaseGame => null;

        // TODO: Localise?
        public string LauncherImagePath => "EnglishSplash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "InstallPath")
        };

        public string Identifier { get; } = "bfme2";

        public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");
        public IControlBarSource ControlBar { get; }

        public static Bfme2Definition Instance { get; } = new Bfme2Definition();
    }
}
