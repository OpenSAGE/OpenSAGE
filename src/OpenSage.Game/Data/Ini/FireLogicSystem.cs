using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class FireLogicSystem
    {
        internal static FireLogicSystem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireLogicSystem> FieldParseTable = new IniParseTable<FireLogicSystem>
        {
            { "MaxCellsBurning", (parser, x) => x.MaxCellsBurning = parser.ParseInteger() },
            { "TerrainCellType", (parser, x) => x.TerrainCellTypes.Add(TerrainCellType.Parse(parser)) }
        };

        public int MaxCellsBurning { get; private set; }
        public List<TerrainCellType> TerrainCellTypes { get; } = new List<TerrainCellType>();
    }

    public class TerrainCellType
    {
        internal static TerrainCellType Parse(IniParser parser)
        {
            return parser.ParseIndexedBlock(
                (x, index) => x.Index = index,
                FieldParseTable);
        }

        private static readonly IniParseTable<TerrainCellType> FieldParseTable = new IniParseTable<TerrainCellType>
        {
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "Name", (parser, x) => x.Name = parser.ParseQuotedString() },
            { "Fuel", (parser, x) => x.Fuel = parser.ParseInteger() },
            { "MaxBurnRate", (parser, x) => x.MaxBurnRate = parser.ParseInteger() },
            { "Decay", (parser, x) => x.Decay = parser.ParseInteger() },
            { "Resistance", (parser, x) => x.Resistance = parser.ParseInteger() },
        };

        public int Index { get; private set; }

        public ColorRgb Color { get; private set; }
        public string Name { get; private set; }
        public int Fuel { get; private set; }
        public int MaxBurnRate { get; private set; }
        public int Decay { get; private set; }
        public int Resistance { get; private set; }
    }
}
