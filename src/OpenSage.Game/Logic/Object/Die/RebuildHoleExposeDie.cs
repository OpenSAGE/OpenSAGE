using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RebuildHoleExposeDie : DieModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// Requires the object specified in <see cref="HoleName"/> to have the REBUILD_HOLE KindOf and 
    /// <see cref="RebuildHoleBehaviorModuleData"/> module in order to work.
    /// </summary>
    public sealed class RebuildHoleExposeDieModuleData : DieModuleData
    {
        internal static RebuildHoleExposeDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RebuildHoleExposeDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<RebuildHoleExposeDieModuleData>
            {
                { "HoleName", (parser, x) => x.HoleName = parser.ParseAssetReference() },
                { "HoleMaxHealth", (parser, x) => x.HoleMaxHealth = parser.ParseFloat() },
                { "FadeInTimeSeconds", (parser, x) => x.FadeInTimeSeconds = parser.ParseFloat() },
                { "TransferAttackers", (parser, x) => x.TransferAttackers = parser.ParseBoolean() }
            });

        public string HoleName { get; private set; }
        public float HoleMaxHealth { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float FadeInTimeSeconds { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool TransferAttackers { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new RebuildHoleExposeDie();
        }
    }
}
