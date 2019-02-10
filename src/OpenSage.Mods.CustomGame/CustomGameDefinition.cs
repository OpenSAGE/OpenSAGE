using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;

namespace OpenSage.Mods.CustomGame
{
    public class CustomGameDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.CustomGame;
        public string DisplayName => "Custom OpenSage Game";
        public IGameDefinition BaseGame => null;

        public bool LauncherImagePrefixLang => true;
        public string LauncherImagePath => "Splash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; }

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; }

        public IEnumerable<RegistryKeyPath> UserDataLeafName { get; }

        public string Identifier { get; } = "CustomGame";

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }

        public static CustomGameDefinition Instance { get; } = new CustomGameDefinition();
    }
}
