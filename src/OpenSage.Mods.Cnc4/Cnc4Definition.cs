﻿using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.Cnc4
{
    public class Cnc4Definition : IGameDefinition
    {
        public SageGame Game => SageGame.Cnc4;
        public string DisplayName => "Command & Conquer (tm) 4: Tiberian Twilight";
        public IGameDefinition BaseGame => null;

        public string LauncherImagePath => null;

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command Conquer 4 Tiberian Twilight", "Install Dir"), // Origin
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\command and conquer 4", "install dir") // Steam
        };

        public string Identifier { get; } = "cnc4";

        public string UserDataLeafNameFallback { get; }

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }

        public static Cnc4Definition Instance { get; } = new Cnc4Definition();
    }
}
