using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.FX
{
    public abstract class FXNugget : DisposableBase
    {
        internal abstract void Execute(FXListContext context);
    }

    public abstract class FXNuggetData
    {
        internal static readonly IniParseTable<FXNuggetData> FXNuggetFieldParseTable = new IniParseTable<FXNuggetData>
        {
            { "ExcludedSourceModelConditions", (parser, x) => x.ExcludedSourceModelConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "RequiredSourceModelConditions", (parser, x) => x.RequiredSourceModelConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) },
            { "SourceObjectFilter", (parser, x) => x.SourceObjectFilter = ObjectFilter.Parse(parser) },
            { "StopIfNuggetPlayed", (parser, x) => x.StopIfPlayed = parser.ParseBoolean() },
            { "Weather", (parser, x) => x.Weather = parser.ParseEnum<WeatherType>() },
            { "OnlyIfOnLand", (parser, x) => x.OnlyIfOnLand = parser.ParseBoolean() },
        };

        public BitArray<ModelConditionFlag> ExcludedSourceModelConditions { get; private set; }
        public BitArray<ModelConditionFlag> RequiredSourceModelConditions { get; private set; }

        // TODO: What is the difference between ObjectFilter and SourceObjectFilter?
        // BFME I's fxlist.ini uses both.
        public ObjectFilter ObjectFilter { get; private set; }
        public ObjectFilter SourceObjectFilter { get; private set; }

        public bool StopIfPlayed { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeatherType Weather { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool OnlyIfOnLand { get; private set; }

        internal virtual FXNugget CreateNugget() => null; // TODO: This should be abstract.
    }
}
