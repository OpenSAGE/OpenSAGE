using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Tools.AptEditor
{
    internal class AptEditorDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Ra3;
        public string DisplayName => "Apt Editor";
        public IGameDefinition BaseGame => null;
        public bool LauncherImagePrefixLang => false;
        public string LauncherImagePath => null;
        public IEnumerable<RegistryKeyPath> RegistryKeys => null;
        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys => null;
        public IMainMenuSource MainMenu => null;
        public IControlBarSource ControlBar => null;
        public string Identifier => "AptEditor";
    }
}