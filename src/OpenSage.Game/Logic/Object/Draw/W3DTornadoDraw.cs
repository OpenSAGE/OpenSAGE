using OpenSage.Data.Ini;
using OpenSage.Gui.InGame;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class W3dTornadoDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dTornadoDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dTornadoDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dTornadoDrawModuleData>
            {
                { "DecalTemplate", (parser, x) => x.DecalTemplate = RadiusDecalTemplate.Parse(parser) },
                { "DecalCount", (parser, x) => x.DecalCount = parser.ParseInteger() },
                { "DecalMaxRadius", (parser, x) => x.DecalMaxRadius = parser.ParseFloat() }
            });

        public RadiusDecalTemplate DecalTemplate { get; private set; }
        public int DecalCount { get; private set; }
        public float DecalMaxRadius { get; private set; }
    }
}
