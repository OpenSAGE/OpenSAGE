using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AutoHealBehaviorModuleData : UpgradeModuleData
    {
        internal static AutoHealBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AutoHealBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<AutoHealBehaviorModuleData>
            {
                { "HealingAmount", (parser, x) => x.HealingAmount = parser.ParseFloat() },
                { "HealingDelay", (parser, x) => x.HealingDelay = parser.ParseInteger() },
                { "AffectsWholePlayer", (parser, x) => x.AffectsWholePlayer = parser.ParseBoolean() },
                { "KindOf", (parser, x) => x.KindOf = parser.ParseEnum<ObjectKinds>() },
                { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
                { "StartHealingDelay", (parser, x) => x.StartHealingDelay = parser.ParseInteger() },
                { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
                { "SingleBurst", (parser, x) => x.SingleBurst = parser.ParseBoolean() },
                { "SkipSelfForHealing", (parser, x) => x.SkipSelfForHealing = parser.ParseBoolean() },
                { "HealOnlyIfNotInCombat", (parser, x) => x.HealOnlyIfNotInCombat = parser.ParseBoolean() },
                { "ButtonTriggered", (parser, x) => x.ButtonTriggered = parser.ParseBoolean() },
                { "HealOnlyOthers", (parser, x) => x.HealOnlyOthers = parser.ParseBoolean() },
                { "UnitHealPulseFX", (parser, x) => x.UnitHealPulseFX = parser.ParseAssetReference() },
                { "AffectsContained", (parser, x) => x.AffectsContained = parser.ParseBoolean() },
                { "NonStackable", (parser, x) => x.NonStackable = parser.ParseBoolean() },
                { "RespawnNearbyHordeMembers", (parser, x) => x.RespawnNearbyHordeMembers = parser.ParseBoolean() },
                { "RespawnFXList", (parser, x) => x.RespawnFXList = parser.ParseAssetReference() },
                { "RespawnMinimumDelay", (parser, x) => x.RespawnMinimumDelay = parser.ParseInteger() },
                { "HealOnlyIfNotUnderAttack", (parser, x) => x.HealOnlyIfNotUnderAttack = parser.ParseBoolean() }
            });

        public float HealingAmount { get; private set; }
        public int HealingDelay { get; private set; }
        public bool AffectsWholePlayer { get; private set; }
        public ObjectKinds KindOf { get; private set; }
        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int StartHealingDelay { get; private set; }
        public float Radius { get; private set; }
        public bool SingleBurst { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool SkipSelfForHealing { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HealOnlyIfNotInCombat { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ButtonTriggered { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HealOnlyOthers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string UnitHealPulseFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AffectsContained { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NonStackable { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool RespawnNearbyHordeMembers { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string RespawnFXList { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int RespawnMinimumDelay { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool HealOnlyIfNotUnderAttack { get; private set; }
    }
}
