using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyGainCreateModuleData : CreateModuleData
    {
        internal static VeterancyGainCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<VeterancyGainCreateModuleData> FieldParseTable = new IniParseTable<VeterancyGainCreateModuleData>
        {
            { "StartingLevel", (parser, x) => x.StartingLevel = parser.ParseEnum<VeterancyLevel>() },
            { "ScienceRequired", (parser, x) => x.ScienceRequired = parser.ParseAssetReference() }
        };

        public VeterancyLevel StartingLevel { get; private set; }
        public string ScienceRequired { get; private set; }
    }
}
