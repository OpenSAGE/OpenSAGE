﻿using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage
{
    public interface IGameDefinition
    {
        SageGame Game { get; }
        string DisplayName { get; }
        IGameDefinition BaseGame { get; }

        string LauncherImagePath { get; }

        IEnumerable<RegistryKeyPath> RegistryKeys { get; }
        IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; }
        
        IMainMenuSource MainMenu { get; }
        IControlBarSource ControlBar { get; }

        string Identifier { get; }
    }
}
