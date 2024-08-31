using System.Collections.Generic;
using FixedMath.NET;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class PropagandaTowerBehavior : UpdateModule
    {
        protected override LogicFrameSpan FramesBetweenUpdates => _moduleData.DelayBetweenUpdates;

        private readonly GameObject _gameObject;
        private readonly PropagandaTowerBehaviorModuleData _moduleData;
        private uint _unknownFrame;
        private readonly List<uint> _objectIds = new();
        public IReadOnlyList<uint> ObjectsInRange => _objectIds;

        private readonly BitArray<ObjectKinds> _allowedKinds = new();

        public PropagandaTowerBehavior(GameObject gameObject, PropagandaTowerBehaviorModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            _allowedKinds.Set(ObjectKinds.Infantry, true);
            _allowedKinds.Set(ObjectKinds.Vehicle, true);
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            if (_gameObject.IsBeingConstructed())
            {
                return;
            }

            foreach (var unitId in ObjectsInRange)
            {
                var unit = GameObjectForId(unitId);
                if (unit.HealedByObjectId == _gameObject.ID)
                {
                    // reset these units, in case they're now out of range
                    unit.HealedByObjectId = 0;
                }
            }

            _objectIds.Clear();

            var fx = GetFxList();

            fx.Value.Execute(context);

            foreach (var candidate in _gameObject.GameContext.Quadtree.FindNearby(_gameObject, _gameObject.Transform, _moduleData.Radius))
            {
                if (!_moduleData.AffectsSelf && candidate == _gameObject) continue;
                if (!CanHealUnit(candidate)) continue;
                _objectIds.Add(candidate.ID);
                HealUnit(candidate);
            }
        }

        private GameObject GameObjectForId(uint unitId)
        {
            return _gameObject.GameContext.GameLogic.GetObjectById(unitId);
        }

        private void HealUnit(GameObject gameObject)
        {
            if (gameObject.HealthPercentage < Fix64.One)
            {
                gameObject.Heal(GetHealPercentage(), _gameObject);
            }
        }

        private bool CanHealUnit(GameObject gameObject)
        {
            return _allowedKinds.Intersects(gameObject.Definition.KindOf) &&
                   ObjectIsOnSameTeam(gameObject) && // todo: does propagandatowerbehavior affect teammates?
                   ObjectNotInContainer(gameObject) &&
                   ObjectNotBeingHealedByAnybodyElse(gameObject);
        }

        private bool ObjectNotInContainer(GameObject gameObject)
        {
            return gameObject.ContainerId == 0; // todo: I believe this should only apply when the container is an enclosing container
        }

        private bool ObjectNotBeingHealedByAnybodyElse(GameObject gameObject)
        {
            return gameObject.HealedByObjectId == 0 || gameObject.HealedByObjectId == _gameObject.ID;
        }

        private bool ObjectIsOnSameTeam(GameObject gameObject)
        {
            return ObjectIsOwnedBySamePlayer(gameObject) ||
                   gameObject.Owner.Allies.Contains(_gameObject.Owner);
        }

        private bool ObjectIsOwnedBySamePlayer(GameObject gameObject)
        {
            return gameObject.Owner == _gameObject.Owner;
        }

        private Percentage GetHealPercentage()
        {
            return _gameObject.HasUpgrade(_moduleData.UpgradeRequired.Value)
                ? _moduleData.UpgradedHealPercentEachSecond
                : _moduleData.HealPercentEachSecond;
        }

        private LazyAssetReference<FXList> GetFxList()
        {
            return _gameObject.HasUpgrade(_moduleData.UpgradeRequired.Value)
                ? _moduleData.UpgradedPulseFX
                : _moduleData.PulseFX;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistFrame(ref _unknownFrame);

            reader.PersistList(
                _objectIds,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistObjectIDValue(ref item);
                });
        }
    }

    public sealed class PropagandaTowerBehaviorModuleData : BehaviorModuleData
    {
        internal static PropagandaTowerBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PropagandaTowerBehaviorModuleData> FieldParseTable = new IniParseTable<PropagandaTowerBehaviorModuleData>
        {
            { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
            { "DelayBetweenUpdates", (parser, x) => x.DelayBetweenUpdates = parser.ParseTimeMillisecondsToLogicFrames() },
            { "HealPercentEachSecond", (parser, x) => x.HealPercentEachSecond = parser.ParsePercentage() },
            { "PulseFX", (parser, x) => x.PulseFX = parser.ParseFXListReference() },
            { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseUpgradeReference() },
            { "UpgradedHealPercentEachSecond", (parser, x) => x.UpgradedHealPercentEachSecond = parser.ParsePercentage() },
            { "UpgradedPulseFX", (parser, x) => x.UpgradedPulseFX = parser.ParseFXListReference() },
            { "AffectsSelf", (parser, x) => x.AffectsSelf = parser.ParseBoolean() },
        };

        public float Radius { get; private set; }
        public LogicFrameSpan DelayBetweenUpdates { get; private set; }
        public Percentage HealPercentEachSecond { get; private set; }
        /// <summary>
        /// plays as often as DelayBetweenUpdates
        /// </summary>
        public LazyAssetReference<FXList> PulseFX { get; private set; }
        public LazyAssetReference<UpgradeTemplate> UpgradeRequired { get; private set; }
        /// <summary>
        /// Percentage to heal if <see cref="UpgradeRequired"/> is owned
        /// </summary>
        public Percentage UpgradedHealPercentEachSecond { get; private set; }
        /// <summary>
        /// plays as often as DelayBetweenUpdates
        /// </summary>
        public LazyAssetReference<FXList> UpgradedPulseFX { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool AffectsSelf { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new PropagandaTowerBehavior(gameObject, this);
        }
    }
}
