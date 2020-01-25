﻿using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;

namespace OpenSage
{
    public interface IGameDefinition
    {
        SageGame Game { get; }
        string DisplayName { get; }
        IGameDefinition BaseGame { get; }

        bool LauncherImagePrefixLang { get; }
        string LauncherImagePath { get; }
        string LauncherExecutable { get; }

        IEnumerable<RegistryKeyPath> RegistryKeys { get; }
        IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; }
        
        IMainMenuSource MainMenu { get; }
        IControlBarSource ControlBar { get; }

        string Identifier { get; }

        OnDemandAssetLoadStrategy CreateAssetLoadStrategy();
    }
}
