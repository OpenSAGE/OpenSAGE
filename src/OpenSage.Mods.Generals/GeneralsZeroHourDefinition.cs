using System.Collections.Generic;
using System.IO;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.CommandListOverlay;
using OpenSage.Gui.Wnd;
using OpenSage.Logic.OrderGenerators;
using OpenSage.Mods.Generals.Gui;
using OpenSage.Mods.Generals.Logic.OrderGenerators;

namespace OpenSage.Mods.Generals
{
    public class GeneralsZeroHourDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.CncGeneralsZeroHour;
        public string DisplayName => "Command & Conquer (tm): Generals - Zero Hour";
        public IGameDefinition BaseGame => GeneralsDefinition.Instance;

        public bool LauncherImagePrefixLang => false;
        public string LauncherImagePath => "Install_Final.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer The First Decade", "zh_folder"),
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "InstallPath"),
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command and Conquer Generals Zero Hour", "Install Dir", "Command and Conquer Generals Zero Hour\\")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer The First Decade", "Language"),
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "Language"),
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command and Conquer Generals Zero Hour", "Language")
        };

        public string Identifier { get; } = "cnc_generals_zh";

        public IMainMenuSource MainMenu { get; } = new WndMainMenuSource(@"Menus\MainMenu.wnd");
        public IControlBarSource ControlBar { get; } = new GeneralsControlBarSource();
        public ICommandListOverlaySource CommandListOverlay => null;

        public uint ScriptingTicksPerSecond => 30;

        public string GetLocalizedStringsPath(string language) => Path.Combine("Data", language, "generals");

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
        {
            return new OnDemandAssetLoadStrategy(PathResolvers.W3d, PathResolvers.GeneralsTexture);
        }

        public bool Probe(string directory)
        {
            return File.Exists(Path.Combine(directory, LauncherExecutable)) && File.Exists(Path.Combine(directory, "INIZH.big"));
        }

        public static GeneralsZeroHourDefinition Instance { get; } = new GeneralsZeroHourDefinition();

        public IOrderGenerator CreateNewOrderGenerator(Game game) => new GeneralsOrderGenerator(game); // todo: zerohourordergenerator?

        public string LauncherExecutable => "generals.exe";
    }
}
