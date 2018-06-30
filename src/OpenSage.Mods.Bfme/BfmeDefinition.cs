using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.BFME
{
    public class BfmeDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Bfme;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm)";
        public IGameDefinition BaseGame => null;

        // TODO: Localise?
        public string LauncherImagePath => "englishsplash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\The Battle for Middle-earth", "InstallPath")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; }

        public string Identifier { get; } = "bfme";

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }

        public static BfmeDefinition Instance { get; } = new BfmeDefinition();
    }
}
