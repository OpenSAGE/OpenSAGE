﻿using System;
using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;

namespace OpenSage.Mods.BfmeII
{
    public class BfmeIIDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.BattleForMiddleEarthII;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm) II";
        public IGameDefinition BaseGame => null;

        public Type WndCallbackType => null;

        public string LauncherImagePath => "*Splash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "InstallPath")
        };

        public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");

        public static BfmeIIDefinition Instance { get; } = new BfmeIIDefinition();
    }
}
