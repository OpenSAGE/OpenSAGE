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
        };

        public List<Side> SidesAllowed { get; internal set; } = new List<Side>();
        public bool UseTheNewCastleSystemInsteadOfTheClunkyBuildList { get; internal set; }
        public ObjectFilter FilterValidOwnedEntries { get; internal set; }
        public bool UseSecondaryBuildList { get; private set; }
        public List<Faction> CastleToUnpackForFactions { get; private set; } = new List<Faction>();
        public float MaxCastleRadius { get; private set; }
        public float FadeTime { get; private set; }
        public int ScanDistance { get; private set; }
        public PreBuildObject PreBuiltList { get; private set; }
        public string PreBuiltPlayer { get; private set; }
        public ObjectFilter FilterCrew { get; private set; }
        public string CrewReleaseFX { get; private set; }
        public string CrewPrepareFX { get; private set; }
        public int CrewPrepareInterval { get; private set; }
    }

    public sealed class Faction
    {
        internal static Faction Parse(IniParser parser)
        {
            var result = new Faction
            {
                FactionName = parser.ParseString(),
                Camp = parser.ParseAssetReference(),
                MaybeStartMoney = parser.GetIntegerOptional()
            };
            return result;
        }

        public string FactionName { get; private set; }
        public string Camp { get; private set; }
        public int MaybeStartMoney { get; private set; }
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

        public string SideName { get; internal set; }
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

        public string ObjectName { get; internal set; }
        public int Count { get; private set; }
    }
}
