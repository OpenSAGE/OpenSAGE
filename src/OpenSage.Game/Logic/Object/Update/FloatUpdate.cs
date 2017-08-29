using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Forces object to remain relative to sea level and allows for use of SEA_LEVEL locomotion 
    /// rules. Setting <see cref="Enabled"/> to <code>true</code> means "float on water and stay 
    /// relative to water level" while setting <see cref="Enabled"/> to <code>false</code> means 
    /// "float on water and bob about".
    /// </summary>
    public sealed class FloatUpdateModuleData : UpdateModuleData
    {
        internal static FloatUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FloatUpdateModuleData> FieldParseTable = new IniParseTable<FloatUpdateModuleData>
        {
            { "Enabled", (parser, x) => x.Enabled = parser.ParseBoolean() }
        };

        public bool Enabled { get; private set; }
    }
}
