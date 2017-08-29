using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of <see cref="RadarUpdateModuleData"/> module on this object if present and enables the 
    /// Radar in the command bar.
    /// </summary>
    public sealed class RadarUpgradeModuleData : UpgradeModuleData
    {
        internal static RadarUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RadarUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<RadarUpgradeModuleData>
            {
                { "DisableProof", (parser, x) => x.DisableProof = parser.ParseBoolean() }
            });

        public bool DisableProof { get; private set; }
    }
}
