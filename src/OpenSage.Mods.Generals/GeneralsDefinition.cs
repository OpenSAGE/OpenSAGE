using System;
using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;

namespace OpenSage.Mods.Generals
{
    public class GeneralsDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.CncGenerals;
        public string DisplayName => "Command & Conquer (tm): Generals";
        public IGameDefinition BaseGame => null;

        public Type WndCallbackType => typeof(WndCallbacks);

        public string LauncherImagePath => "Install_Final.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Generals", "InstallPath")
        };

        public IMainMenuSource MainMenu => new WndMainMenuSource(@"Menus\MainMenu.wnd");

        public static GeneralsDefinition Instance { get; } = new GeneralsDefinition();
    }
}
