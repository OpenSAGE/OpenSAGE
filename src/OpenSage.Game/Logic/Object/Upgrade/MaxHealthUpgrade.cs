using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class MaxHealthUpgradeModuleData : UpgradeModuleData
    {
        internal static MaxHealthUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<MaxHealthUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<MaxHealthUpgradeModuleData>
            {
                { "AddMaxHealth", (parser, x) => x.AddMaxHealth = parser.ParseFloat() },
                { "ChangeType", (parser, x) => x.ChangeType = parser.ParseEnum<MaxHealthChangeType>() },
            });

        public float AddMaxHealth { get; private set; }
        public MaxHealthChangeType ChangeType { get; private set; }
    }

    public enum MaxHealthChangeType
    {
        [IniEnum("PRESERVE_RATIO")]
        PreserveRatio,

        [IniEnum("ADD_CURRENT_HEALTH_TOO")]
        AddCurrentHealthToo
    }
}
