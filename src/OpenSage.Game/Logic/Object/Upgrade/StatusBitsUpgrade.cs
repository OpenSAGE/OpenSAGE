﻿using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class StatusBitsUpgradeModuleData : UpgradeModuleData
    {
        internal static StatusBitsUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StatusBitsUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<StatusBitsUpgradeModuleData>
            {
                { "StatusToSet", (parser, x) => x.StatusToSet = parser.ParseEnumBitArray<ObjectStatus>() }
            });

        [AddedIn(SageGame.Bfme2Rotwk)]
        public BitArray<ObjectStatus> StatusToSet { get; private set; }
    }
}
