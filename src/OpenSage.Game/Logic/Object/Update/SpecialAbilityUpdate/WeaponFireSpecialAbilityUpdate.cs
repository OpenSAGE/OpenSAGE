using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WeaponFireSpecialAbilityUpdateModuleData : SpecialAbilityUpdateModuleData
    {
        internal new static WeaponFireSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<WeaponFireSpecialAbilityUpdateModuleData> FieldParseTable = SpecialAbilityUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<WeaponFireSpecialAbilityUpdateModuleData>
        {
            { "WhichSpecialWeapon", (parser, x) => x.WhichSpecialWeapon = parser.ParseInteger() },
            { "SkipContinue", (parser, x) => x.SkipContinue = parser.ParseBoolean() },
            { "MustFinishAbility", (parser, x) => x.MustFinishAbility = parser.ParseBoolean() },
            { "SpecialWeapon", (parser, x) => x.SpecialWeapon = parser.ParseAssetReference() },
            { "BusyForDuration", (parser, x) => x.BusyForDuration = parser.ParseInteger() },
            { "PlayWeaponPreFireFX", (parser, x) => x.PlayWeaponPreFireFX = parser.ParseBoolean() },
            { "ApproachUntilMembersInRange", (parser, x) => x.ApproachUntilMembersInRange = parser.ParseBoolean() },
            { "ChainedButton", (parser, x) => x.ChainedButton = parser.ParseAssetReference() },
            { "NeedLivingTargets", (parser, x) => x.NeedLivingTargets = parser.ParseBoolean() }
        });

        public int WhichSpecialWeapon { get; private set; }
        public bool SkipContinue { get; private set; }
        public bool MustFinishAbility { get; private set; }
        public string SpecialWeapon { get; private set; }
        public int BusyForDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool PlayWeaponPreFireFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ApproachUntilMembersInRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ChainedButton { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NeedLivingTargets { get; private set; }
    }
}
