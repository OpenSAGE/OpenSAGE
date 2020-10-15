using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.UnitOverlay;

namespace OpenSage.Mods.CnC3
{
    public class Cnc3KanesWrathDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Cnc3KanesWrath;
        public string DisplayName => "Command & Conquer (tm) 3: Kane's Wrath";
        public IGameDefinition BaseGame => Cnc3Definition.Instance;

        public bool LauncherImagePrefixLang => false;
        public string LauncherImagePath => @"Launcher\splash.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Command and Conquer 3 Kanes Wrath", "InstallPath")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; }

        public string Identifier { get; } = "cnc3_kw";

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }
        public IUnitOverlaySource UnitOverlay => null;

        public uint ScriptingTicksPerSecond => 5;

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy() => OnDemandAssetLoadStrategy.None;

        public static Cnc3KanesWrathDefinition Instance { get; } = new Cnc3KanesWrathDefinition();

        public string LauncherExecutable => "CNC3EP1.exe";
    }
}
