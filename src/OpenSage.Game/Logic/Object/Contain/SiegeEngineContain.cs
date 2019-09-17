using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class SiegeEngineContainModuleData : BehaviorModuleData
    {
        internal static SiegeEngineContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SiegeEngineContainModuleData> FieldParseTable = new IniParseTable<SiegeEngineContainModuleData>
        {
           { "ObjectStatusOfCrew", (parser, x) => x.ObjectStatusOfCrew = parser.ParseEnumBitArray<ObjectStatus>() },
           { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
           { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
           { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) },
           { "KillPassengersOnDeath", (parser, x) => x.KillPassengersOnDeath = parser.ParseBoolean() },
           { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
           { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() },
           { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
           { "CrewFilter", (parser, x) => x.CrewFilter = ObjectFilter.Parse(parser) },
           { "CrewMax", (parser, x) => x.CrewMax = parser.ParseInteger() },
           { "InitialCrew", (parser, x) => x.InitialCrew = Crew.Parse(parser) },
           { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
           { "NumberOfExitPaths", (parser, x) => x.NumberOfExitPaths = parser.ParseInteger() },
           { "GoAggressiveOnExit", (parser, x) => x.GoAggressiveOnExit = parser.ParseBoolean() }, 
           { "TypeOneForWeaponSet", (parser, x) => x.TypeOneForWeaponSet = parser.ParseEnum<ObjectKinds>() },
           { "EjectPassengersOnDeath", (parser, x) => x.EjectPassengersOnDeath = parser.ParseBoolean() },
           { "PassengerBonePrefix", (parser, x) => x.PassengerBonePrefixes.Add(PassengerBonePrefix.Parse(parser)) },
           { "BoneSpecificConditionState", (parser, x) => x.BoneSpecificConditionStates.Add(BoneSpecificConditionState.Parse(parser)) },
           { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
           { "ShowPips", (parser, x) => x.ShowPips = parser.ParseBoolean() },
           { "SpeedPercentPerCrew", (parser, x) => x.SpeedPercentPerCrew = parser.ParsePercentage() }
        };

        public BitArray<ObjectStatus> ObjectStatusOfCrew { get; private set; }
        public int Slots { get; private set; }
        public Percentage DamagePercentToUnits { get; private set; }
        public ObjectFilter PassengerFilter { get; private set; }
        public bool KillPassengersOnDeath { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public ObjectFilter CrewFilter { get; private set; }
        public int CrewMax { get; private set; }
        public Crew InitialCrew { get; private set; }
        public int ExitDelay { get; private set; }
        public int NumberOfExitPaths { get; private set; }
        public bool GoAggressiveOnExit { get; private set; }
        public ObjectKinds TypeOneForWeaponSet { get; private set; }
        public bool EjectPassengersOnDeath { get; private set; }
        public List<PassengerBonePrefix> PassengerBonePrefixes { get; } = new List<PassengerBonePrefix>();
        public List<BoneSpecificConditionState> BoneSpecificConditionStates { get; } = new List<BoneSpecificConditionState>();
        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public bool ShowPips { get; private set; }
        public Percentage SpeedPercentPerCrew { get; private set; }
    }

    public struct Crew
    {
        internal static Crew Parse(IniParser parser)
        {
            return new Crew()
            {
                CrewObject = parser.ParseAssetReference(),
                NumMembers = parser.ParseInteger()
            };
        }

        public string CrewObject { get; private set; }
        public int NumMembers { get; private set; }
    }

    public struct BoneSpecificConditionState
    {
        internal static BoneSpecificConditionState Parse(IniParser parser)
        {
            return new BoneSpecificConditionState()
            {
                ID = parser.ParseInteger(),
                Condition = parser.ParseEnum<ModelConditionFlag>()
            };
        }

        public int ID { get; private set; }
        public ModelConditionFlag Condition { get; private set; }
    }
}
