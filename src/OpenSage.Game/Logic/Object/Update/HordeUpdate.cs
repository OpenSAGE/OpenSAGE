#nullable enable

using System.Linq;
using ImGuiNET;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class HordeUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly HordeUpdateModuleData _moduleData;

        private bool _isInHorde;

        protected override LogicFrameSpan FramesBetweenUpdates => _moduleData.UpdateRate;

        internal HordeUpdate(GameObject gameObject, GameContext context, HordeUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            var nearby = _context.Scene3D.Quadtree.FindNearby(_gameObject, _gameObject.Transform, _moduleData.Radius);
            var nearbyInConstraints = nearby.Where(MatchesAlliance).Where(MatchesType).Select(o => o.FindBehavior<HordeUpdate>()).ToList();

            if (nearbyInConstraints.Count >= _moduleData.Count - 1) // this list doesn't include us, but count does
            {
                foreach (var nearbyItem in nearbyInConstraints)
                {
                    // enable horde mode iff count succeeds
                    nearbyItem.AddToHorde();
                }
                AddToHorde();
            }
            else if (_isInHorde)
            {
                // check if there are others who are in a horde near us
                // I don't think this behavior is strictly correct, but it seems the simplest way to implement this logic in the spirit of the data names and descriptions
                var nearbyRubOff = _context.Scene3D.Quadtree.FindNearby(_gameObject, _gameObject.Transform, _moduleData.RubOffRadius);
                var nearbyRubOffInConstraints = nearbyRubOff.Where(MatchesAlliance).Where(MatchesType).Count(o => o.FindBehavior<HordeUpdate>() is { _isInHorde: true });
                if (nearbyRubOffInConstraints < _moduleData.Count - 1) // this list doesn't include us, but count does
                {
                    RemoveFromHorde();
                }
            }
        }

        private void AddToHorde()
        {
            _isInHorde = true;
            _gameObject.Drawable.ObjectDecalType = GameObjectDecalType;
            _gameObject.AddWeaponBonusType(_moduleData.Action);
        }

        private void RemoveFromHorde()
        {
            _isInHorde = false;
            _gameObject.Drawable.ObjectDecalType = ObjectDecalType.None;
            _gameObject.RemoveWeaponBonusType(_moduleData.Action);
        }

        // todo: should this be hardcoded?
        private UpgradeTemplate? NationalismUpgrade => _context.AssetLoadContext.AssetStore.Upgrades.GetLazyAssetReferenceByName("Upgrade_Nationalism")?.Value;

        private bool HasNationalism => NationalismUpgrade != null && _gameObject.Owner.HasUpgrade(NationalismUpgrade);

        private ObjectDecalType GameObjectDecalType => HasNationalism switch
        {
            true => _gameObject.IsKindOf(ObjectKinds.Infantry) ? ObjectDecalType.NationalismInfantry : ObjectDecalType.NationalismVehicle,
            false => _gameObject.IsKindOf(ObjectKinds.Infantry) ? ObjectDecalType.HordeInfantry : ObjectDecalType.HordeVehicle,
        };

        private bool MatchesAlliance(GameObject gameObject)
        {
            return !_moduleData.AlliesOnly || gameObject.Owner == _gameObject.Owner || gameObject.Owner.Allies.Contains(_gameObject.Owner);
        }

        private bool MatchesType(GameObject gameObject)
        {
            return _moduleData.ExactMatch
                ? gameObject.Definition == _gameObject.Definition // if the definitions match, then we have everything we need
                : gameObject.FindBehavior<HordeUpdate>() != null && // otherwise, we need the horde update behavior
                  gameObject.Definition.KindOf.Intersects(_moduleData.KindOf); // and the kindof needs to match
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistBoolean(ref _isInHorde);

            reader.SkipUnknownBytes(1);
        }

        internal override void DrawInspector()
        {
            base.DrawInspector();
            ImGui.LabelText("In horde", _isInHorde.ToString());
        }
    }

    /// <summary>
    /// Hardcoded to apply the following textures to objects that are affected by this module:
    /// EXHorde, EXHorde_UP, EXHordeB, EXHordeB_UP.
    /// </summary>
    public sealed class HordeUpdateModuleData : UpdateModuleData
    {
        internal static HordeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HordeUpdateModuleData> FieldParseTable = new IniParseTable<HordeUpdateModuleData>
        {
            { "RubOffRadius", (parser, x) => x.RubOffRadius = parser.ParseInteger() },
            { "UpdateRate", (parser, x) => x.UpdateRate = parser.ParseTimeMillisecondsToLogicFrames() },
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "AlliesOnly", (parser, x) => x.AlliesOnly = parser.ParseBoolean() },
            { "ExactMatch", (parser, x) => x.ExactMatch = parser.ParseBoolean() },
            { "Count", (parser, x) => x.Count = parser.ParseInteger() },
            { "Action", (parser, x) => x.Action = parser.ParseEnum<WeaponBonusType>() }
        };

        /// <summary>
        /// if I am this close to a real hordesman, I will get to be an honorary hordesman
        /// </summary>
        public int RubOffRadius { get; private set; }
        /// <summary>
        /// how often to recheck horde status (msec)
        /// </summary>
        public LogicFrameSpan UpdateRate { get; private set; }
        /// <summary>
        /// how close other units must be to us to count towards our horde-ness
        /// </summary>
        public int Radius { get; private set; }
        /// <summary>
        /// what KindOf's must match to count towards horde-ness
        /// </summary>
        public BitArray<ObjectKinds> KindOf { get; private set; } = new();
        /// <summary>
        /// do we only count allies towards horde status?
        /// </summary>
        public bool AlliesOnly { get; private set; }
        /// <summary>
        /// do we only count units of our exact same type towards horde status? (overrides kindof)
        /// </summary>
        public bool ExactMatch { get; private set; }
        /// <summary>
        /// how many units must be within Radius to grant us horde-ness
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// when horde-ing, grant us the HORDE bonus
        /// </summary>
        public WeaponBonusType Action { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new HordeUpdate(gameObject, context, this);
        }
    }
}
