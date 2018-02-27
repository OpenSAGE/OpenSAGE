using System;
using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage
{
    public interface IGameDefinition
    {
        SageGame Game { get; }
        string DisplayName { get; }
        IGameDefinition BaseGame { get; }

        Type WndCallbackType { get; }

        string LauncherImagePath { get; }

        IEnumerable<RegistryKeyPath> RegistryKeys { get; }

        IMainMenuSource MainMenu { get; }
    }
}
