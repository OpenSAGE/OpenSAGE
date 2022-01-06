using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyGainCreate : CreateModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new VeterancyGainCreate();
        }
    }
}
