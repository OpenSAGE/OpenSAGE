﻿using System.Collections.Generic;
using System.IO;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.CommandListOverlay;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd;
using OpenSage.Mods.Generals.Gui;

namespace OpenSage.Mods.Generals;

public class GeneralsDefinition : IGameDefinition
{
    public SageGame Game => SageGame.CncGenerals;
    public string DisplayName => "Command & Conquer (tm): Generals";
    public IGameDefinition BaseGame => null;

    public bool LauncherImagePrefixLang => false;
    public string LauncherImagePath => "Install_Final.bmp";

    public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
    {
        new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Generals", "InstallPath")
    };

    public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
    {
        new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\Generals", "Language")
    };

    public SteamInstallationDefinition Steam { get; } = new(2229870, "Command and Conquer Generals");

    public string Identifier { get; } = "cnc_generals";

    public IMainMenuSource MainMenu { get; } = new WndMainMenuSource(@"Menus\MainMenu.wnd");
    public IControlBarSource ControlBar { get; } = new GeneralsControlBarSource();
    public ICommandListOverlaySource CommandListOverlay => null;

    public uint ScriptingTicksPerSecond => 30;

    public string GetLocalizedStringsPath(string language) => Path.Combine("Data", language, "generals");

    public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
    {
        return new OnDemandAssetLoadStrategy(PathResolvers.W3d, PathResolvers.GeneralsTexture);
    }

    public Scene25D CreateScene25D(IScene3D scene3D, AssetStore assetStore) => new GeneralsScene25D(scene3D, assetStore);

    public bool Probe(string directory)
    {
        return File.Exists(Path.Combine(directory, LauncherExecutable)) && File.Exists(Path.Combine(directory, "INI.big"));
    }

    public static GeneralsDefinition Instance { get; } = new GeneralsDefinition();

    public string LauncherExecutable => "generals.exe";
}
