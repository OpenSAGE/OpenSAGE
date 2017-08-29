using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Dynamically adjusts the "bomb" so it hits its designated target instead of missing it.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SmartBombTargetHomingUpdateModuleData : UpdateModuleData
    {
        internal static SmartBombTargetHomingUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SmartBombTargetHomingUpdateModuleData> FieldParseTable = new IniParseTable<SmartBombTargetHomingUpdateModuleData>
        {
            { "CourseCorrectionScalar", (parser, x) => x.CourseCorrectionScalar = parser.ParseFloat() },
        };

        public float CourseCorrectionScalar { get; private set; }
    }
}
