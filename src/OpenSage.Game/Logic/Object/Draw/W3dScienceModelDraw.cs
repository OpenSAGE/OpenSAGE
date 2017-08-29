using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class W3dScienceModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dScienceModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dScienceModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dScienceModelDrawModuleData>
            {
                { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAssetReference() }
            });

        public string RequiredScience { get; private set; }
    }
}
