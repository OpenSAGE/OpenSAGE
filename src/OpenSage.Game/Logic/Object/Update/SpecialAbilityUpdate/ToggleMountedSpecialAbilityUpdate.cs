using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ToggleMountedSpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static ToggleMountedSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ToggleMountedSpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<ToggleMountedSpecialAbilityUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseString() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "OpacityTarget", (parser, x) => x.OpacityTarget = parser.ParseFloat() },
            { "TriggerInstantlyOnCreate", (parser, x) => x.TriggerInstantlyOnCreate = parser.ParseBoolean() },
            { "CancelDisguiseWhenDismounting", (parser, x) => x.CancelDisguiseWhenDismounting = parser.ParseBoolean() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
            { "MountedTemplate", (parser, x) => x.MountedTemplate = parser.ParseAssetReference() },
            { "SynchronizeTimerOnSpecialPower", (parser, x) => x.SynchronizeTimerOnSpecialPower = parser.ParseAssetReferenceArray() },
            { "IgnoreFacingCheck", (parser, x) => x.IgnoreFacingCheck = parser.ParseBoolean() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public float OpacityTarget { get; private set; }
        public bool TriggerInstantlyOnCreate { get; private set; }
        public bool CancelDisguiseWhenDismounting { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float StartAbilityRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string MountedTemplate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] SynchronizeTimerOnSpecialPower { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IgnoreFacingCheck { get; private set; }
    }
}
