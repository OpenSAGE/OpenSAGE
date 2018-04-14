using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.Ra3
{
    public class Ra3Definition : IGameDefinition
    {
        public SageGame Game => SageGame.Ra3;
        public string DisplayName => "Command & Conquer (tm): Red Alert (tm) 3";
        public IGameDefinition BaseGame => null;

        public string LauncherImagePath => @"Launcher\splash.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Red Alert 3", "Install Dir"),
        };

        public string Identifier { get; } = "ra3";

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }

        public static Ra3Definition Instance { get; } = new Ra3Definition();
    }
}
