using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AutoAbilityBehaviorModuleData : UpgradeModuleData
    {
        internal static AutoAbilityBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AutoAbilityBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<AutoAbilityBehaviorModuleData>
        {
            { "SpecialAbility", (parser, x) => x.SpecialAbility = parser.ParseAssetReference() },
            { "BaseMaxRangeFromStartPos", (parser, x) => x.BaseMaxRangeFromStartPos = parser.ParseBoolean() },
            { "AdjustAttackMeleePosition", (parser, x) => x.AdjustAttackMeleePosition = parser.ParseBoolean() },
            { "MaxScanRange", (parser, x) => x.MaxScanRange = parser.ParseInteger() },
            { "MinScanRange", (parser, x) => x.MinScanRange = parser.ParseInteger() },
            { "AllowSelf", (parser, x) => x.AllowSelf = parser.ParseBoolean() },
            { "IdleTimeSeconds", (parser, x) => x.IdleTimeSeconds = parser.ParseInteger() },
            { "Query", (parser, x) => x.Querys.Add(Query.Parse(parser)) },
            { "ForbiddenStatus", (parser, x) => x.ForbiddenStatus = parser.ParseEnum<ObjectStatus>() }
        });

        [AddedIn(SageGame.Bfme2)]
        public string SpecialAbility { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool BaseMaxRangeFromStartPos { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AdjustAttackMeleePosition { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MaxScanRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MinScanRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AllowSelf { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int IdleTimeSeconds { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<Query> Querys { get; } = new List<Query>();

        [AddedIn(SageGame.Bfme2)]
        public ObjectStatus ForbiddenStatus { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class Query
    {
        internal static Query Parse(IniParser parser)
        {
            return new Query
            {
                Unknown = parser.ParseInteger(),
                ObjectFilter = ObjectFilter.Parse(parser)
            };
        }

        public int Unknown { get; private set; }

        public ObjectFilter ObjectFilter { get; private set; }
    }
}
