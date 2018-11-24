using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public abstract class ClientBehaviorModuleData : BehaviorModuleData
    {
        internal static ClientBehaviorModuleData ParseClientBehavior(IniParser parser) => ParseModule(parser, ClientBehaviorParseTable);

        private static readonly Dictionary<string, Func<IniParser, ClientBehaviorModuleData>> ClientBehaviorParseTable = new Dictionary<string, Func<IniParser, ClientBehaviorModuleData>>
        {
            { "AnimationSoundClientBehavior", AnimationSoundClientBehaviorData.Parse },
            { "UpgradeSoundSelectorClientBehavior", UpgradeSoundSelectorClientBehaviorData.Parse }
        };
    }
}
