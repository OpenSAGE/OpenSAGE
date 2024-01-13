using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CreateCrateDie : DieModule<CreateCrateDieModuleData>
    {
        // TODO
        public CreateCrateDie(CreateCrateDieModuleData moduleData) : base(moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class CreateCrateDieModuleData : DieModuleData
    {
        internal static CreateCrateDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CreateCrateDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<CreateCrateDieModuleData>
            {
                { "CrateData", (parser, x) => x.CrateData = parser.ParseCrateReference() }
            });

        public LazyAssetReference<CrateData> CrateData { get; private set; }
    }
}
