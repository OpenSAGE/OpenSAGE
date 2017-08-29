using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class SpecialPowerModuleData : BehaviorModuleData
    {
        internal static readonly IniParseTable<SpecialPowerModuleData> FieldParseTable = new IniParseTable<SpecialPowerModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "StartsPaused", (parser, x) => x.StartsPaused = parser.ParseBoolean() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public bool StartsPaused { get; private set; }
    }
}
