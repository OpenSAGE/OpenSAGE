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
        };

        public List<Side> SidesAllowed { get; internal set; } = new List<Side>();
        public bool UseTheNewCastleSystemInsteadOfTheClunkyBuildList { get; internal set; }
        public ObjectFilter FilterValidOwnedEntries { get; internal set; }
        public bool UseSecondaryBuildList { get; private set; }
        public List<Faction> CastleToUnpackForFactions { get; private set; } = new List<Faction>();
        public float MaxCastleRadius { get; private set; }
        public float FadeTime { get; private set; }
        public int ScanDistance { get; private set; }
    }

    public sealed class Faction
    {
        internal static Faction Parse(IniParser parser)
        {
            var result = new Faction();
            result.FactionName = parser.ParseString();
            result.Camp = parser.ParseAssetReference();

            var unknown = parser.GetNextTokenOptional();
            if (unknown.HasValue)
            {
                result.MaybeStartMoney = parser.ScanInteger(unknown.Value);
            }
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


}
