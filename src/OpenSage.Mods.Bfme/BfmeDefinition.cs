using System;
using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.BFME
{
    public class BfmeDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.BattleForMiddleEarth;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm)";
        public IGameDefinition BaseGame => null;

        public Type WndCallbackType => null;

        public string LauncherImagePath => "*splash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\The Battle for Middle-earth", "InstallPath")
        };

        public IMainMenuSource MainMenu { get; }

        public static BfmeDefinition Instance { get; } = new BfmeDefinition();
    }
}
