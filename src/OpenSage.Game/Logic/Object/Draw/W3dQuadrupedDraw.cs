using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dQuadrupedDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dQuadrupedDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly new IniParseTable<W3dQuadrupedDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dQuadrupedDrawModuleData>
            {
                { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
                { "LeftFrontFootBone", (parser, x) => x.LeftFrontFootBone = parser.ParseAssetReference() },
                { "RightFrontFootBone", (parser, x) => x.RightFrontFootBone = parser.ParseAssetReference() },
                { "LeftRearFootBone", (parser, x) => x.LeftRearFootBone = parser.ParseAssetReference() },
                { "RightRearFootBone", (parser, x) => x.RightRearFootBone = parser.ParseAssetReference() },
            });

        public bool StaticModelLODMode { get; private set; }
        public string LeftFrontFootBone { get; private set; }
        public string RightFrontFootBone { get; private set; }
        public string LeftRearFootBone { get; private set; }
        public string RightRearFootBone { get; private set; }
    }
}
