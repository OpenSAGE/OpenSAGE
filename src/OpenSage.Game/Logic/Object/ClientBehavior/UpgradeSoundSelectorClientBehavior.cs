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

        public List<SoundUpgrade> SoundUpgrades { get; internal set; } = new List<SoundUpgrade>();
    }


    public sealed class SoundUpgrade
    {
        internal static SoundUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SoundUpgrade> FieldParseTable = new IniParseTable<SoundUpgrade>
        {
            { "VoiceSelect", (parser, x) => x.VoiceSelect = parser.ParseAssetReference() },
            { "ExcludedUpgrades", (parser, x) => x.ExcludedUpgrades = ParseExcludedUpgrades(parser) }
        };

        private static List<string> ParseExcludedUpgrades(IniParser parser)
        {
            var result = new List<string>();
            var upgrade = parser.ParseString();
            while (upgrade != string.Empty)
            {
                result.Add(upgrade);
                upgrade = parser.ParseString();
            }
            return result;
        }

        public string VoiceSelect { get; internal set; }
        public List<string> ExcludedUpgrades { get; internal set; } = new List<string>();
    }
}
