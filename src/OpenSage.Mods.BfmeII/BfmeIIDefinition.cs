using System;
using System.Collections.Generic;
using OpenSage.Data;

namespace OpenSage.Mods.BfmeII
{
    public class BfmeIIDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.BattleForMiddleEarth;
        public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm) 2";
        public IGameDefinition BaseGame => null;

        public Type WndCallbackType => null;

        // TODO: Localise?
        public string LauncherImagePath => "EnglishSplash.jpg";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\The Battle for Middle-earth II", "InstallPath")
        };

        public static BfmeIIDefinition Instance { get; } = new BfmeIIDefinition();
    }
}
