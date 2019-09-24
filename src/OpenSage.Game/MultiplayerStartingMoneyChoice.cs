﻿using OpenSage.Data.Ini;

namespace OpenSage
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class MultiplayerStartingMoneyChoice : BaseAsset
    {
        internal static MultiplayerStartingMoneyChoice Parse(IniParser parser)
        {
            var result = parser.ParseTopLevelBlock(FieldParseTable);
            result.SetNameAndInstanceId("MultiplayerStartingMoneyChoice", result.Value.ToString());
            return result;
        }

        private static readonly IniParseTable<MultiplayerStartingMoneyChoice> FieldParseTable = new IniParseTable<MultiplayerStartingMoneyChoice>
        {
            { "Value", (parser, x) => x.Value = parser.ParseInteger() },
            { "Default", (parser, x) => x.Default = parser.ParseBoolean() }
        };

        public int Value { get; private set; }
        public bool Default { get; private set; }
    }
}
