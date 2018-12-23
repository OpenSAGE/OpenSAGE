﻿using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the object specified in <see cref="HoleName"/> to have the REBUILD_HOLE KindOf and 
    /// <see cref="RebuildHoleBehaviorModuleData"/> module in order to work.
    /// </summary>
    public sealed class RebuildHoleExposeDieModuleData : DieModuleData
    {
        internal static RebuildHoleExposeDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RebuildHoleExposeDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<RebuildHoleExposeDieModuleData>
            {
                { "HoleName", (parser, x) => x.HoleName = parser.ParseAssetReference() },
                { "HoleMaxHealth", (parser, x) => x.HoleMaxHealth = parser.ParseFloat() },
                { "FadeInTimeSeconds", (parser, x) => x.FadeInTimeSeconds = parser.ParseFloat() },
            });

        public string HoleName { get; private set; }
        public float HoleMaxHealth { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float FadeInTimeSeconds { get; private set; }
    }
}
