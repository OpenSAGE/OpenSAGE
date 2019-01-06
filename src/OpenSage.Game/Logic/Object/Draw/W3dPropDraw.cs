using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class W3dPropDrawModuleData : DrawModuleData
    {
        internal static W3dPropDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<W3dPropDrawModuleData> FieldParseTable = new IniParseTable<W3dPropDrawModuleData>
        {
            { "ModelName", (parser, x) => x.ModelName = parser.ParseAssetReference() },
        };

        public string ModelName { get; private set; }
    }
}
