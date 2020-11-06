using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.UnitOverlay;

namespace OpenSage.Tools.AptEditor
{
    internal class AptEditorDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Ra3;
        public string DisplayName => "Apt Editor";
        public IGameDefinition BaseGame => null;
        public bool LauncherImagePrefixLang => false;
        public string LauncherImagePath => null;
        public IEnumerable<RegistryKeyPath> RegistryKeys => Enumerable.Empty<RegistryKeyPath>();
        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys => null;
        public IMainMenuSource MainMenu => null;
        public IControlBarSource ControlBar => null;
        public string Identifier => "AptEditor";
        public string LauncherExecutable => throw new NotSupportedException("This is apt editor");
        public IUnitOverlaySource UnitOverlay => throw new NotSupportedException("This is apt editor");
        public uint ScriptingTicksPerSecond => 1; // actually not used

        public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
        {
            return new OnDemandAssetLoadStrategy(PathResolvers.W3d, PathResolvers.Bfme2Texture);
        }
    }
}
