using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires ExitStartN and ExitEndN bones defined unless overridden by <see cref="ExitBone"/>.
    /// Allows the use of SoundEnter And SoundExit UnitSpecificSounds.
    /// </summary>
    public sealed class TransportContain : ObjectBehavior
    {
        internal static TransportContain Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TransportContain> FieldParseTable = new IniParseTable<TransportContain>
        {
            { "PassengersAllowedToFire", (parser, x) => x.PassengersAllowedToFire = parser.ParseBoolean() },
            { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
            { "HealthRegen%PerSec", (parser, x) => x.HealthRegenPercentPerSecond = parser.ParseInteger() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
            { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
            { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
            { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() },
            { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
            { "NumberOfExitPaths", (parser, x) => x.NumberOfExitPaths = parser.ParseInteger() },
            { "GoAggressiveOnExit", (parser, x) => x.GoAggressiveOnExit = parser.ParseBoolean() },
            { "DoorOpenTime", (parser, x) => x.DoorOpenTime = parser.ParseInteger() },
            { "ScatterNearbyOnExit", (parser, x) => x.ScatterNearbyOnExit = parser.ParseBoolean() },
            { "OrientLikeContainerOnExit", (parser, x) => x.OrientLikeContainerOnExit = parser.ParseBoolean() },
            { "KeepContainerVelocityOnExit", (parser, x) => x.KeepContainerVelocityOnExit = parser.ParseBoolean() },
            { "ExitPitchRate", (parser, x) => x.ExitPitchRate = parser.ParseInteger() },
            { "ExitBone", (parser, x) => x.ExitBone = parser.ParseBoneName() },
            { "DestroyRidersWhoAreNotFreeToExit", (parser, x) => x.DestroyRidersWhoAreNotFreeToExit = parser.ParseBoolean() }
        };

        public bool PassengersAllowedToFire { get; private set; }
        public int Slots { get; private set; }
        public int HealthRegenPercentPerSecond { get; private set; }
        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
        public float DamagePercentToUnits { get; private set; }
        public BitArray<ObjectKinds> AllowInsideKindOf { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
        public int ExitDelay { get; private set; }
        public int NumberOfExitPaths { get; private set; }
        public bool GoAggressiveOnExit { get; private set; }
        public int DoorOpenTime { get; private set; }
        public bool ScatterNearbyOnExit { get; private set; }
        public bool OrientLikeContainerOnExit { get; private set; }
        public bool KeepContainerVelocityOnExit { get; private set; }
        public int ExitPitchRate { get; private set; }
        public string ExitBone { get; private set; }
        public bool DestroyRidersWhoAreNotFreeToExit { get; private set; }
    }
}
