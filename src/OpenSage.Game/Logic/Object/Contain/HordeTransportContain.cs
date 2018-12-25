using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class HordeTransportContainModuleData : BehaviorModuleData
    {
        internal static HordeTransportContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<HordeTransportContainModuleData> FieldParseTable = new IniParseTable<HordeTransportContainModuleData>
        {
           { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
           { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
           { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
           { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
           { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
           { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) },
           { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() },
           { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
           { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
           { "NumberOfExitPaths", (parser, x) => x.NumberOfExitPaths = parser.ParseInteger() },
           { "ForceOrientationContainer", (parser, x) => x.ForceOrientationContainer = parser.ParseBoolean() },
           { "PassengerBonePrefix", (parser, x) => x.PassengerBonePrefix = PassengerBonePrefix.Parse(parser) },
           { "EjectPassengersOnDeath", (parser, x) => x.EjectPassengersOnDeath = parser.ParseBoolean() },
           { "AllowOwnPlayerInsideOverride", (parser, x) => x.AllowOwnPlayerInsideOverride = parser.ParseBoolean() },
           { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
           { "ShowPips", (parser, x) => x.ShowPips = parser.ParseBoolean() },
           { "FadeFilter", (parser,x) => x.FadeFilter = ObjectFilter.Parse(parser) },
           { "FadePassengerOnEnter", (parser, x) => x.FadePassengerOnEnter = parser.ParseBoolean() },
           { "EnterFadeTime", (parser, x) => x.EnterFadeTime = parser.ParseInteger() },
           { "FadePassengerOnExit", (parser, x) => x.FadePassengerOnExit = parser.ParseBoolean() },
           { "ExitFadeTime", (parser, x) => x.ExitFadeTime = parser.ParseInteger() },
           { "KillPassengersOnDeath", (parser, x) => x.KillPassengersOnDeath = parser.ParseBoolean() },
           { "InitialPayload", (parser, x) => x.InitialPayloads.Add(Payload.Parse(parser)) },
        };

        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public int Slots { get; private set; }
        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
        public float DamagePercentToUnits { get; private set; }
        public ObjectFilter PassengerFilter { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public int ExitDelay { get; private set; }
        public int NumberOfExitPaths { get; private set; }
        public bool ForceOrientationContainer { get; private set; }
        public PassengerBonePrefix PassengerBonePrefix { get; private set; }
        public bool EjectPassengersOnDeath { get; private set; }
        public bool AllowOwnPlayerInsideOverride { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public bool ShowPips { get; private set; }
        public ObjectFilter FadeFilter { get; private set; }
        public bool FadePassengerOnEnter { get; private set; }
        public int EnterFadeTime { get; private set; }
        public bool FadePassengerOnExit { get; private set; }
        public int ExitFadeTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool KillPassengersOnDeath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<Payload> InitialPayloads { get; } = new List<Payload>();
    }
}
