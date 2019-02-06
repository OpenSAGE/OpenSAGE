using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RegionCampain
    {
        internal static RegionCampain Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<RegionCampain> FieldParseTable = new IniParseTable<RegionCampain>
        {
            { "RegionObject", (parser, x) => x.RegionObject = parser.ParseAssetReference() },
            { "Region", (parser, x) => x.Regions.Add(Region.Parse(parser)) }
        };

        public string Name { get; private set; }

        public string RegionObject { get; private set; }
        public List<Region> Regions { get; } = new List<Region>();
    }

    public sealed class Region
    {
        internal static Region Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Region> FieldParseTable = new IniParseTable<Region>
        {
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseQuotedString() },
            { "MapName", (parser, x) => x.MapName = parser.ParseQuotedString() },
            { "MovieNameFirstTime", (parser, x) => x.MovieNameFirstTime = parser.ParseAssetReference() },
            { "MovieNameRepeat", (parser, x) => x.MovieNameRepeat = parser.ParseAssetReference() },
            { "SubObject", (parser, x) => x.SubObject = parser.ParseAssetReference() },
        };

        public string Name { get; private set; }

        public string DisplayName { get; private set; }
        public string MapName { get; private set; }
        public string MovieNameFirstTime { get; private set; }
        public string MovieNameRepeat { get; private set; }
        public string SubObject { get; private set; }
    }
}
