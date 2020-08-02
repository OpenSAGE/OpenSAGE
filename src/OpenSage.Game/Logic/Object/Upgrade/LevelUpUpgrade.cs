﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LevelUpUpgradeModuleData : UpgradeModuleData
    {
        internal static LevelUpUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<LevelUpUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<LevelUpUpgradeModuleData>
            {
                { "LevelsToGain", (parser, x) => x.LevelsToGain = parser.ParseInteger() },
                { "LevelCap", (parser, x) => x.LevelCap = parser.ParseInteger() }
            });

        public int LevelsToGain { get; private set; }
        public int LevelCap { get; private set; }
    }
}
