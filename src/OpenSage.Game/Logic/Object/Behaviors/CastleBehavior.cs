using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class CastleBehaviorModuleData : BehaviorModuleData
    {
        internal static CastleBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<CastleBehaviorModuleData> FieldParseTable = new IniParseTable<CastleBehaviorModuleData>
        {
            { "SidesAllowed", (parser, x) => x.SidesAllowed.Add(Side.Parse(parser)) },
            { "UseTheNewCastleSystemInsteadOfTheClunkyBuildList", (parser, x) => x.UseTheNewCastleSystemInsteadOfTheClunkyBuildList = parser.ParseBoolean() },
            { "FilterValidOwnedEntries", (parser, x) => x.FilterValidOwnedEntries = ObjectFilter.Parse(parser) },
            { "UseSecondaryBuildList", (parser, x) => x.UseSecondaryBuildList = parser.ParseBoolean() },
            { "CastleToUnpackForFaction", (parser, x) => x.CastleToUnpackForFactions.Add(Faction.Parse(parser)) },
            { "MaxCastleRadius", (parser, x) => x.MaxCastleRadius = parser.ParseFloat() },
            { "FadeTime", (parser, x) => x.FadeTime = parser.ParseFloat() },
            { "ScanDistance", (parser, x) => x.ScanDistance = parser.ParseInteger() },
            { "PreBuiltList", (parser, x) => x.PreBuiltList = PreBuildObject.Parse(parser) },
            { "PreBuiltPlyr", (parser, x) => x.PreBuiltPlayer = parser.ParseString() },
            { "FilterCrew", (parser, x) => x.FilterCrew = ObjectFilter.Parse(parser) },
            { "CrewReleaseFX", (parser, x) => x.CrewReleaseFX = parser.ParseAssetReference() },
            { "CrewPrepareFX", (parser, x) => x.CrewPrepareFX = parser.ParseAssetReference() },
            { "CrewPrepareInterval", (parser, x) => x.CrewPrepareInterval = parser.ParseInteger() },
            { "DisableStructureRotation", (parser, x) => x.DisableStructureRotation = parser.ParseBoolean() },
            { "FactionDecal", (parser, x) => x.FactionDecals.Add(Faction.Parse(parser)) },
            { "InstantUnpack", (parser, x) => x.InstantUnpack = parser.ParseBoolean() },
            { "KeepDeathKillsEverything", (parser, x) => x.KeepDeathKillsEverything = parser.ParseBoolean() },
            { "EvaEnemyCastleSightedEvent", (parser, x) => x.EvaEnemyCastleSightedEvent = parser.ParseAssetReference() },
            { "UnpackDelayTime", (parser, x) => x.UnpackDelayTime = parser.ParseFloat() },
            { "Summoned", (parser, x) => x.Summoned = parser.ParseBoolean() }
        };

        public List<Side> SidesAllowed { get;} = new List<Side>();
        public bool UseTheNewCastleSystemInsteadOfTheClunkyBuildList { get; private set; }
        public ObjectFilter FilterValidOwnedEntries { get; private set; }
        public bool UseSecondaryBuildList { get; private set; }
        public List<Faction> CastleToUnpackForFactions { get; } = new List<Faction>();
        public float MaxCastleRadius { get; private set; }
        public float FadeTime { get; private set; }
        public int ScanDistance { get; private set; }
        public PreBuildObject PreBuiltList { get; private set; }
        public string PreBuiltPlayer { get; private set; }
        public ObjectFilter FilterCrew { get; private set; }
        public string CrewReleaseFX { get; private set; }
        public string CrewPrepareFX { get; private set; }
        public int CrewPrepareInterval { get; private set; }
        public bool DisableStructureRotation { get; private set; }
        public List<Faction> FactionDecals { get; } = new List<Faction>();

        [AddedIn(SageGame.Bfme2)]
        public bool InstantUnpack { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool KeepDeathKillsEverything { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string EvaEnemyCastleSightedEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float UnpackDelayTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool Summoned { get; private set; }
    }

    public sealed class Faction
    {
        internal static Faction Parse(IniParser parser)
        {
            var result = new Faction
            {
                FactionName = parser.ParseString(),
                Camp = parser.ParseAssetReference(),
                MaybeStartMoney = parser.GetFloatOptional()
            };
            return result;
        }

        public string FactionName { get; private set; }
        public string Camp { get; private set; }
        public float MaybeStartMoney { get; private set; }
    }

    public sealed class Side
    {
        internal static Side Parse(IniParser parser)
        {
            return new Side()
            {
                SideName = parser.ParseString(),
                CommandSourceTypes = parser.ParseEnumFlags<CommandSourceTypes>()
            };
        }

        public string SideName { get; private set; }
        public CommandSourceTypes CommandSourceTypes { get; private set; }
    }

    public sealed class PreBuildObject
    {
        internal static PreBuildObject Parse(IniParser parser)
        {
            return new PreBuildObject()
            {
                ObjectName = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string ObjectName { get; private set; }
        public int Count { get; private set; }
    }
}
