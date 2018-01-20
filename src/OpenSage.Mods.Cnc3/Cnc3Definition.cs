using System;
using System.Collections.Generic;
using OpenSage.Data;

namespace OpenSage.Mods.CnC3
{
    public class Cnc3Definition : IGameDefinition
    {
        public SageGame Game => SageGame.Cnc3;
        public string DisplayName => "Command & Conquer (tm) 3: Tiberium Wars";
        public IGameDefinition BaseGame => null;

        public Type WndCallbackType => null;

        public string LauncherImagePath => @"Launcher\splash.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3", "InstallPath")
        };

        public static Cnc3Definition Instance { get; } = new Cnc3Definition();
    }
}
