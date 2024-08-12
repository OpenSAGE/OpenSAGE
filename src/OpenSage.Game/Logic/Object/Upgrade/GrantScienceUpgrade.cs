using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class GrantScienceUpgrade : UpgradeModule
    {
        public GrantScienceUpgrade(GameObject gameObject, UpgradeModuleData moduleData)
            : base(gameObject, moduleData)
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

    public sealed class GrantScienceUpgradeModuleData : UpgradeModuleData
    {
        internal static GrantScienceUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GrantScienceUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<GrantScienceUpgradeModuleData>
            {
                { "GrantScience", (parser, x) => x.GrantScience = parser.ParseAssetReference() },
            });

        public string GrantScience { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GrantScienceUpgrade(gameObject, this);
        }
    }
}
