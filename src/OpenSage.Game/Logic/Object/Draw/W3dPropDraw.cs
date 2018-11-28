using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class W3dPropDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dPropDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dPropDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dPropDrawModuleData>
            {
                 { "ModelName", (parser, x) => x.ModelName = parser.ParseAssetReference() },
            });

        public string ModelName { get; private set; }
    }
}
