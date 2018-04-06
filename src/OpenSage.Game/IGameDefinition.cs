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

        IMainMenuSource MainMenu { get; }
        IControlBarSource ControlBar { get; }

        string Identifier { get; }

        // This is used as the fallback if UserDataLeafName is not present in GameData.ini.
        string UserDataLeafNameFallback { get; }
    }
}
