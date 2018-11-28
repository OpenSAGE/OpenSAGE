using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class UpgradeSoundSelectorClientBehaviorData : ClientBehaviorModuleData
    {
        internal static UpgradeSoundSelectorClientBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<UpgradeSoundSelectorClientBehaviorData> FieldParseTable = new IniParseTable<UpgradeSoundSelectorClientBehaviorData>
        {
            { "SoundUpgrade", (parser, x) => x.SoundUpgrades.Add(SoundUpgrade.Parse(parser)) }
        };

        public List<SoundUpgrade> SoundUpgrades { get; private set; } = new List<SoundUpgrade>();
    }


    public sealed class SoundUpgrade
    {
        internal static SoundUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SoundUpgrade> FieldParseTable = new IniParseTable<SoundUpgrade>
        {
            { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAssetReference() },
            { "ExcludedUpgrades", (parser, x) => x.ExcludedUpgrades = parser.ParseAssetReferenceArray() }
        };

        public string VoiceSelect { get; private set; }
        public string[] ExcludedUpgrades { get; private set; }
    }
}
