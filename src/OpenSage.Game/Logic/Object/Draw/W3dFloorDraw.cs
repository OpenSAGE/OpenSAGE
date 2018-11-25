using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dFloorDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dFloorDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dFloorDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dFloorDrawModuleData>
            {
                { "ModelName", (parser, x) => x.ModelName = parser.ParseAssetReference() },
                { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
                { "StartHidden", (parser, x) => x.StartHidden = parser.ParseBoolean() },
                { "ForceToBack", (parser, x) => x.ForceToBack = parser.ParseBoolean() }
            });

        public string ModelName { get; internal set; }
        public bool StaticModelLODMode { get; internal set; }
        public bool StartHidden { get; internal set; }
        public bool ForceToBack { get; internal set; }
    }
}
