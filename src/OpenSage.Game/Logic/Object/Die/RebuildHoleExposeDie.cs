using FixedMath.NET;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RebuildHoleExposeDie : DieModule
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly RebuildHoleExposeDieModuleData _moduleData;

        internal RebuildHoleExposeDie(GameObject gameObject, GameContext context, RebuildHoleExposeDieModuleData moduleData) : base(moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;
        }

        private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
        {
            var hole = _context.GameLogic.CreateObject(_moduleData.HoleDefinition.Value, _gameObject.Owner);
            hole.SetTransformMatrix(_gameObject.TransformMatrix);
            var holeHealth = (Fix64)_moduleData.HoleMaxHealth;
            hole.MaxHealth = holeHealth;
            hole.Health = holeHealth;
            hole.FindBehavior<RebuildHoleUpdate>().SetOriginalStructure(_gameObject);

            base.Die(context, deathType);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// Requires the object specified in <see cref="HoleDefinition"/> to have the REBUILD_HOLE KindOf and
    /// <see cref="RebuildHoleUpdateModuleData"/> module in order to work.
    /// </summary>
    public sealed class RebuildHoleExposeDieModuleData : DieModuleData
    {
        internal static RebuildHoleExposeDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RebuildHoleExposeDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<RebuildHoleExposeDieModuleData>
            {
                { "HoleName", (parser, x) => x.HoleDefinition = parser.ParseObjectReference() },
                { "HoleMaxHealth", (parser, x) => x.HoleMaxHealth = parser.ParseFloat() },
                { "FadeInTimeSeconds", (parser, x) => x.FadeInTimeSeconds = parser.ParseFloat() },
                { "TransferAttackers", (parser, x) => x.TransferAttackers = parser.ParseBoolean() }
            });

        public override ObjectStatus? ExemptStatus { get; protected set; } = ObjectStatus.UnderConstruction;

        public LazyAssetReference<ObjectDefinition> HoleDefinition { get; private set; }
        public float HoleMaxHealth { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float FadeInTimeSeconds { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool TransferAttackers { get; private set; }

        internal override RebuildHoleExposeDie CreateModule(GameObject gameObject, GameContext context)
        {
            return new RebuildHoleExposeDie(gameObject, context, this);
        }
    }
}
