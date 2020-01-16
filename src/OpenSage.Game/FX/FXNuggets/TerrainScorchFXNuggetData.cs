using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class TerrainScorchFXNuggetData : FXNuggetData
    {
        internal static TerrainScorchFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TerrainScorchFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<TerrainScorchFXNuggetData>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<TerrainScorchType>() },
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "RandomRange", (parser, x) => x.RandomRange = parser.ParseVector2() }
        });

        public TerrainScorchType Type { get; private set; }
        public int Radius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Vector2 RandomRange { get; private set; }
    }
}
