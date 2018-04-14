﻿using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Mods.Generals.Gui;

namespace OpenSage.Mods.Generals
{
    public class GeneralsDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.CncGenerals;
        public string DisplayName => "Command & Conquer (tm): Generals";
        public IGameDefinition BaseGame => null;

        public string LauncherImagePath => "Install_Final.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Generals", "InstallPath")
        };

        public string Identifier { get; } = "cnc_generals";

        public IMainMenuSource MainMenu { get; } = new WndMainMenuSource(@"Menus\MainMenu.wnd");
        public IControlBarSource ControlBar { get; } = new GeneralsControlBarSource();

        public static GeneralsDefinition Instance { get; } = new GeneralsDefinition();
    }
}
