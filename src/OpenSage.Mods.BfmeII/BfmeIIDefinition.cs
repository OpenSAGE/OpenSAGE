using System;
using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.BfmeII
{
    public class BfmeIIDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.BattleForMiddleEarthII;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm) II";
        public IGameDefinition BaseGame => null;

        public Type WndCallbackType => null;

        // TODO: Localise?
        public string LauncherImagePath => "EnglishSplash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "InstallPath")
        };

        public IMainMenuSource MainMenu { get; }

        public static BfmeIIDefinition Instance { get; } = new BfmeIIDefinition();
    }
}
