﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CreateObjectDieModuleData : DieModuleData
    {
        internal static CreateObjectDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CreateObjectDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<CreateObjectDieModuleData>
            {
                { "CreationList", (parser, x) => x.CreationList = parser.ParseAssetReference() },
                { "TransferPreviousHealth", (parser, x) => x.TransferPreviousHealth = parser.ParseBoolean() },
                { "DebrisPortionOfSelf", (parser, x) => x.DebrisPortionOfSelf = parser.ParseAssetReference() },
                { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseAssetReferenceArray() }
            });

        public string CreationList { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool TransferPreviousHealth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string DebrisPortionOfSelf { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] UpgradeRequired { get; private set; }
    }
}
