using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;

namespace OpenSage.Mods.CnC3
{
    public class Cnc3KanesWrathDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Cnc3KanesWrath;
        public string DisplayName => "Command & Conquer (tm) 3: Kane's Wrath";
        public IGameDefinition BaseGame => Cnc3Definition.Instance;

        public bool LauncherImagePrefixLang => false;
        public string LauncherImagePath => @"Launcher\splash.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3 Kanes Wrath", "installpath")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3 Kanes Wrath", "language")
        };

        public IEnumerable<RegistryKeyPath> UserDataLeafName { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3 Kanes Wrath", "userdataleafname")
        };

        public string Identifier { get; } = "cnc3_kw";

        public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");
        public IControlBarSource ControlBar { get; }

        public static Cnc3KanesWrathDefinition Instance { get; } = new Cnc3KanesWrathDefinition();
    }
}
