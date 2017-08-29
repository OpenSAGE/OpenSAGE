using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class DeletionUpdateModuleData : UpdateModuleData
    {
        internal static DeletionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DeletionUpdateModuleData> FieldParseTable = new IniParseTable<DeletionUpdateModuleData>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseInteger() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseInteger() }
        };

        public int MinLifetime { get; private set; }
        public int MaxLifetime { get; private set; }
    }
}
