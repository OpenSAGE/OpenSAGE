using System;
using System.Collections.Generic;
using OpenSage.Data;

namespace OpenSage.Mods.CnC3
{
    public class Cnc3KanesWrathDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Cnc3KanesWrath;
        public string DisplayName => "Command & Conquer (tm) 3: Kane's Wrath";
        public IGameDefinition BaseGame => Cnc3Definition.Instance;

        public Type WndCallbackType => null;

        public string LauncherImagePath => @"Launcher\splash.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3 Kanes Wrath", "InstallPath")
        };

        public static Cnc3KanesWrathDefinition Instance { get; } = new Cnc3KanesWrathDefinition();
    }
}
