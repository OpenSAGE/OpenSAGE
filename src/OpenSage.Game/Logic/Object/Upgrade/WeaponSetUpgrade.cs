using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of PLAYER_UPGRADE WeaponSet on this object.
    /// Allows the use of WeaponUpgradeSound within UnitSpecificSounds section of the object.
    /// Allows the use of the WEAPONSET_PLAYER_UPGRADE ConditionState.
    /// </summary>
    public sealed class WeaponSetUpgradeModuleData : UpgradeModuleData
    {
        internal static WeaponSetUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WeaponSetUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<WeaponSetUpgradeModuleData>()
            {
                { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = CustomAnimAndDuration.Parse(parser) },
            });

        [AddedIn(SageGame.Bfme)]
        public CustomAnimAndDuration CustomAnimAndDuration { get; internal set; }
        
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class CustomAnimAndDuration
    {
        internal static CustomAnimAndDuration Parse(IniParser parser)
        {
            return new CustomAnimAndDuration()
            {
                AnimState = parser.ParseAttributeEnum<ModelConditionFlag>("AnimState"),
                AnimTime = parser.ParseAttributeInteger("AnimTime"),
                TriggerTime = parser.ParseAttributeInteger("TriggerTime")
            };
        }

        public ModelConditionFlag AnimState { get; private set; }
        public int AnimTime { get; private set; }
        public int TriggerTime { get; private set; }
    }
}
