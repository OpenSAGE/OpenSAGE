using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ToggleDeploySpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static ToggleDeploySpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ToggleDeploySpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<ToggleDeploySpecialAbilityUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseString() },
            { "IgnoreFacingCheck", (parser, x) => x.IgnoreFacingCheck = parser.ParseBoolean() },
            { "SoundDeploy", (parser, x) => x.SoundDeploy = parser.ParseAssetReference() },
            { "SoundUndeploy", (parser, x) => x.SoundUndeploy = parser.ParseAssetReference() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public bool IgnoreFacingCheck { get; private set; }
        public string SoundDeploy { get; private set; }
        public string SoundUndeploy { get; private set; }
    }
}
