using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires ExitStartN and ExitEndN bones defined unless overridden by <see cref="ExitBone"/>.
    /// Allows the use of SoundEnter And SoundExit UnitSpecificSounds.
    /// </summary>
    public class TransportContainModuleData : OpenContainModuleData
    {
        internal static TransportContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<TransportContainModuleData> FieldParseTable = OpenContainModuleData.FieldParseTable
            .Concat(new IniParseTable<TransportContainModuleData>
            {
                { "PassengersAllowedToFire", (parser, x) => x.PassengersAllowedToFire = parser.ParseBoolean() },
                { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
                { "HealthRegen%PerSec", (parser, x) => x.HealthRegenPercentPerSecond = parser.ParseInteger() },
                { "BurnedDeathToUnits", (parser, x) => x.BurnedDeathToUnits = parser.ParseBoolean() },
                { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
                { "GoAggressiveOnExit", (parser, x) => x.GoAggressiveOnExit = parser.ParseBoolean() },
                { "DoorOpenTime", (parser, x) => x.DoorOpenTime = parser.ParseInteger() },
                { "ScatterNearbyOnExit", (parser, x) => x.ScatterNearbyOnExit = parser.ParseBoolean() },
                { "OrientLikeContainerOnExit", (parser, x) => x.OrientLikeContainerOnExit = parser.ParseBoolean() },
                { "KeepContainerVelocityOnExit", (parser, x) => x.KeepContainerVelocityOnExit = parser.ParseBoolean() },
                { "ExitPitchRate", (parser, x) => x.ExitPitchRate = parser.ParseInteger() },
                { "ExitBone", (parser, x) => x.ExitBone = parser.ParseBoneName() },
                { "DestroyRidersWhoAreNotFreeToExit", (parser, x) => x.DestroyRidersWhoAreNotFreeToExit = parser.ParseBoolean() },
                { "InitialPayload", (parser, x) => x.InitialPayload = Payload.Parse(parser) },
                { "ArmedRidersUpgradeMyWeaponSet", (parser, x) => x.ArmedRidersUpgradeMyWeaponSet = parser.ParseBoolean() },
                { "WeaponBonusPassedToPassengers", (parser, x) => x.WeaponBonusPassedToPassengers = parser.ParseBoolean() },
                { "DelayExitInAir", (parser, x) => x.DelayExitInAir = parser.ParseBoolean() },
                { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
                { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) },
                { "ShowPips", (parser, x) => x.ShowPips = parser.ParseBoolean() },
                { "TypeOneForWeaponSet", (parser, x) => x.TypeOneForWeaponSet = parser.ParseEnum<ObjectKinds>() },
                { "TypeTwoForWeaponSet", (parser, x) => x.TypeTwoForWeaponSet = parser.ParseEnum<ObjectKinds>() },
                { "TypeOneForWeaponState", (parser, x) => x.TypeOneForWeaponState = parser.ParseEnum<ObjectKinds>() },
                { "TypeTwoForWeaponState", (parser, x) => x.TypeTwoForWeaponState = parser.ParseEnum<ObjectKinds>() },
                { "PassengerBonePrefix", (parser, x) => x.PassengerBonePrefixes.Add(PassengerBonePrefix.Parse(parser)) },
                { "KillPassengersOnDeath", (parser, x) => x.KillPassengersOnDeath = parser.ParseBoolean() },
                { "ManualPickUpFilter", (parser, x) => x.ManualPickUpFilter = ObjectFilter.Parse(parser) },
                { "EjectPassengersOnDeath", (parser, x) => x.EjectPassengersOnDeath = parser.ParseBoolean() }
            });

        public bool PassengersAllowedToFire { get; private set; }
        public int Slots { get; private set; }
        public int HealthRegenPercentPerSecond { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool BurnedDeathToUnits { get; private set; }

        public int ExitDelay { get; private set; }
        
        public bool GoAggressiveOnExit { get; private set; }
        public int DoorOpenTime { get; private set; }
        public bool ScatterNearbyOnExit { get; private set; }
        public bool OrientLikeContainerOnExit { get; private set; }
        public bool KeepContainerVelocityOnExit { get; private set; }
        public int ExitPitchRate { get; private set; }
        public string ExitBone { get; private set; }
        public bool DestroyRidersWhoAreNotFreeToExit { get; private set; }
        public Payload InitialPayload { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ArmedRidersUpgradeMyWeaponSet { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool WeaponBonusPassedToPassengers { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool DelayExitInAir { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter PassengerFilter { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShowPips { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectKinds TypeOneForWeaponSet { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectKinds TypeTwoForWeaponSet { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectKinds TypeOneForWeaponState { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectKinds TypeTwoForWeaponState { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<PassengerBonePrefix> PassengerBonePrefixes { get; } = new List<PassengerBonePrefix>();

        [AddedIn(SageGame.Bfme)]
        public bool KillPassengersOnDeath { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter ManualPickUpFilter { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool EjectPassengersOnDeath { get; private set; }
    }

    public sealed class PassengerBonePrefix
    {
        internal static PassengerBonePrefix Parse(IniParser parser)
        {
            var result = new PassengerBonePrefix();
            result.BoneName = parser.ParseAttribute("PassengerBone", parser.ScanBoneName);
            result.ObjectKind = parser.ParseAttributeEnum<ObjectKinds>("KindOf");
            return result;
        }

        public string BoneName { get; private set; }
        public ObjectKinds ObjectKind { get; private set; }
    }
}
