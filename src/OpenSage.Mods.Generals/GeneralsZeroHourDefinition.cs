﻿using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Mods.Generals.Gui;

namespace OpenSage.Mods.Generals
{
    public class GeneralsZeroHourDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.CncGeneralsZeroHour;
        public string DisplayName => "Command & Conquer (tm): Generals - Zero Hour";
        public IGameDefinition BaseGame => GeneralsDefinition.Instance;

        public string LauncherImagePath => "Install_Final.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer The First Decade", "zh_folder"),
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "InstallPath"),
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command and Conquer Generals Zero Hour", "Install Dir", "Command and Conquer Generals Zero Hour\\")
        };

        public string Identifier { get; } = "cnc_generals_zh";

        public string UserDataLeafNameFallback { get; } = "Command and Conquer Generals Zero Hour Data";

        public IMainMenuSource MainMenu { get; } = new WndMainMenuSource(@"Menus\MainMenu.wnd");
        public IControlBarSource ControlBar { get; } = new GeneralsControlBarSource();

        public static GeneralsZeroHourDefinition Instance { get; } = new GeneralsZeroHourDefinition();
    }
}
