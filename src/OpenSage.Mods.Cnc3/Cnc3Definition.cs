﻿using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.CnC3
{
    public class Cnc3Definition : IGameDefinition
    {
        public SageGame Game => SageGame.Cnc3;
        public string DisplayName => "Command & Conquer (tm) 3: Tiberium Wars";
        public IGameDefinition BaseGame => null;

        public string LauncherImagePath => @"Launcher\splash.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3", "InstallPath")
        };

        public string Identifier { get; } = "cnc3";

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }

        public static Cnc3Definition Instance { get; } = new Cnc3Definition();
    }
}
