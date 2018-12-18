using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AttachUpdateModuleData : UpdateModuleData
    {
        internal static AttachUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AttachUpdateModuleData> FieldParseTable = new IniParseTable<AttachUpdateModuleData>
        {
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) }
        };

        public ObjectFilter ObjectFilter { get; private set; }
    }
}
