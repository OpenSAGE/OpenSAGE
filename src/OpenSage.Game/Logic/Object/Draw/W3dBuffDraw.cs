using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class W3dBuffDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dBuffDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dBuffDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dBuffDrawModuleData>
            {
                 { "ModelName", (parser, x) => x.ModelName = parser.ParseAssetReference() },
                 { "StaticModelLODMode", (parser, x) => x.StaticModelLodMode = parser.ParseBoolean() },
                 { "PreDraw", (parser, x) => x.PreDraw = parser.ParseBoolean() }
            });

        public string ModelName { get; private set; }
        public bool StaticModelLodMode { get; private set; }
        public bool PreDraw { get; private set; }
    }
}
