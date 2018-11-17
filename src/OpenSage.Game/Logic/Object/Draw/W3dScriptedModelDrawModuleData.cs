using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dScriptedModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dScriptedModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dScriptedModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dScriptedModelDrawModuleData>
            {
                { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
            });

        public bool StaticModelLODMode { get; private set; }
    }
}
