using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DeletionUpdateModuleData : UpdateModuleData
    {
        internal static DeletionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DeletionUpdateModuleData> FieldParseTable = new IniParseTable<DeletionUpdateModuleData>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseLong() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseLong() }
        };

        public long MinLifetime { get; private set; }
        public long MaxLifetime { get; private set; }
    }
}
