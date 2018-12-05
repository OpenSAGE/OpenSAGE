using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AISpecialPowerUpdateModuleData : AIUpdateModuleData
    {
        internal static new AISpecialPowerUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AISpecialPowerUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<AISpecialPowerUpdateModuleData>
            {
                { "CommandButtonName", (parser, x) => x.CommandButtonName = parser.ParseIdentifier() },
                { "SpecialPowerAIType", (parser, x) => x.SpecialPowerAIType = parser.ParseEnum<SpecialPowerAIType>() }
            });

        public string CommandButtonName { get; private set; }
        public SpecialPowerAIType SpecialPowerAIType { get; private set; }
    }

    public enum SpecialPowerAIType
    {
        [IniEnum("AI_SPECIAL_POWER_CAPTURE_BUILDING")]
        AiSpecialPowerCaptureBuilding,
    }
}
