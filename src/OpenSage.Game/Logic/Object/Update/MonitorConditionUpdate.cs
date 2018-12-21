﻿using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class MonitorConditionUpdateModuleData : UpdateModuleData
    {
        internal static MonitorConditionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MonitorConditionUpdateModuleData> FieldParseTable = new IniParseTable<MonitorConditionUpdateModuleData>
        {
            { "WeaponSetFlags", (parser, x) => x.WeaponSetFlags = parser.ParseEnumBitArray<WeaponSetConditions>() },
            { "WeaponToggleCommandSet", (parser, x) => x.WeaponToggleCommandSet = parser.ParseAssetReference() },
        };

        public BitArray<WeaponSetConditions> WeaponSetFlags { get; private set; }
        public string WeaponToggleCommandSet { get; private set; }
    }
}
