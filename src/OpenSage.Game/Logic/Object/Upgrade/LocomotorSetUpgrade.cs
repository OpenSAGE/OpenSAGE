using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of SET_NORMAL_UPGRADED locomotor on this object and allows the use of 
    /// VoiceMoveUpgrade within the UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class LocomotorSetUpgradeModuleData : UpgradeModuleData
    {
        internal static LocomotorSetUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<LocomotorSetUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<LocomotorSetUpgradeModuleData>());
    }
}
