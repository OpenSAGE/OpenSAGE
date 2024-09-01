using FixedMath.NET;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    // It looks from the .sav files that this actually inherits from UpdateModule,
    // not UpgradeModule (but in the xsds it inherits from UpgradeModule).
    public sealed class AutoHealBehavior : UpdateModule, IUpgradeableModule, ISelfHealable
    {
        protected override LogicFrameSpan FramesBetweenUpdates => _moduleData.HealingDelay;

        private readonly GameObject _gameObject;
        private readonly AutoHealBehaviorModuleData _moduleData;
        private readonly UpgradeLogic _upgradeLogic;
        /// <summary>
        /// This is a guess as to what this frame is for, and may not be correct.
        /// </summary>
        private LogicFrame _endOfStartHealingDelay;

        public AutoHealBehavior(GameObject gameObject, AutoHealBehaviorModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            NextUpdateFrame.Frame = uint.MaxValue;
            _upgradeLogic = new UpgradeLogic(moduleData.UpgradeData, OnUpgrade);
        }

        public bool CanUpgrade(UpgradeSet existingUpgrades) => _upgradeLogic.CanUpgrade(existingUpgrades);

        public void TryUpgrade(UpgradeSet completedUpgrades) => _upgradeLogic.TryUpgrade(completedUpgrades);

        private void OnUpgrade()
        {
            // todo: if unit is max health and this is a self-heal behavior, even if upgrade was triggered, nextupdateframe is still maxvalue
            NextUpdateFrame.Frame = _gameObject.GameContext.GameLogic.CurrentFrame.Value;
        }

        /// <summary>
        /// Increments the frame after which healing is allowed. If <code>_moduleData.StartHealingDelay</code> is 0, this is a no-op.
        /// </summary>
        public void RegisterDamage()
        {
            // this seems to only apply if the unit is capable of healing itself
            // make sure the upgrade is triggered before resetting any frames
            if (_moduleData.StartHealingDelay != LogicFrameSpan.Zero && _upgradeLogic.Triggered)
            {
                var currentFrame = _gameObject.GameContext.GameLogic.CurrentFrame;
                _endOfStartHealingDelay = currentFrame + _moduleData.StartHealingDelay;
                NextUpdateFrame = new UpdateFrame(_endOfStartHealingDelay);
            }
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            if (context.LogicFrame < _endOfStartHealingDelay)
            {
                return;
            }

            if (_moduleData.Radius == 0)
            {
                if (_moduleData.AffectsWholePlayer)
                {
                    // USA hospital has this behavior
                    foreach (var candidate in _gameObject.GameContext.GameLogic.Objects)
                    {
                        if (ObjectIsOwnedBySamePlayer(candidate) && CanHealUnit(candidate))
                        {
                            HealUnit(candidate);
                        }
                    }
                }
                else
                {
                    // we only heal ourselves - china mines and GLA junk repair have this behavior
                    // todo: unclear how to show healing icon for this behavior
                    HealUnit(_gameObject);
                }

                return;
            }

            foreach (var candidate in _gameObject.GameContext.Quadtree.FindNearby(_gameObject, _gameObject.Transform, _moduleData.Radius))
            {
                if (_moduleData.SkipSelfForHealing && candidate == _gameObject) continue;
                if (!CanHealUnit(candidate)) continue;

                HealUnit(candidate);
            }
        }

        // todo: lots of duplicated logic between this and propagandatowerbehavior
        private bool CanHealUnit(GameObject gameObject)
        {
            return _moduleData.ForbiddenKindOf?.Intersects(gameObject.Definition.KindOf) != true &&
                   _moduleData.KindOf?.Intersects(gameObject.Definition.KindOf) != false &&
                   ObjectIsOnSameTeam(gameObject) && // todo: does autohealbehavior affect teammates?
                   ObjectNotInContainer(gameObject) &&
                   ObjectNotBeingHealedByAnybodyElse(gameObject); // don't heal units that are being healed by something else
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
                   gameObject.Owner.Allies.Contains(_gameObject.Owner); // I _think_ this is correct
        }

        private bool ObjectIsOwnedBySamePlayer(GameObject gameObject)
        {
            return gameObject.Owner == _gameObject.Owner;
        }

        private void HealUnit(GameObject gameObject)
        {
            if (gameObject.HealthPercentage < Fix64.One)
            {
                gameObject.Heal((Fix64)_moduleData.HealingAmount, _gameObject);
                if (gameObject != _gameObject)
                {
                    gameObject.SetBeingHealed(_gameObject, NextUpdateFrame.Frame);
                }
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistObject(_upgradeLogic);

            reader.SkipUnknownBytes(4);

            reader.PersistLogicFrame(ref _endOfStartHealingDelay);

            reader.SkipUnknownBytes(1);
        }
    }

    public sealed class AutoHealBehaviorModuleData : UpdateModuleData
    {
        internal static AutoHealBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoHealBehaviorModuleData> FieldParseTable =
            new IniParseTableChild<AutoHealBehaviorModuleData, UpgradeLogicData>(x => x.UpgradeData, UpgradeLogicData.FieldParseTable)
            .Concat(new IniParseTable<AutoHealBehaviorModuleData>
            {
                { "HealingAmount", (parser, x) => x.HealingAmount = parser.ParseFloat() },
                { "HealingDelay", (parser, x) => x.HealingDelay = parser.ParseTimeMillisecondsToLogicFrames() },
                { "AffectsWholePlayer", (parser, x) => x.AffectsWholePlayer = parser.ParseBoolean() },
                { "KindOf", (parser, x) => x.KindOf = parser.ParseEnumBitArray<ObjectKinds>() },
                { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
                { "StartHealingDelay", (parser, x) => x.StartHealingDelay = parser.ParseTimeMillisecondsToLogicFrames() },
                { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
                { "SingleBurst", (parser, x) => x.SingleBurst = parser.ParseBoolean() },
                { "SkipSelfForHealing", (parser, x) => x.SkipSelfForHealing = parser.ParseBoolean() },
                { "HealOnlyIfNotInCombat", (parser, x) => x.HealOnlyIfNotInCombat = parser.ParseBoolean() },
                { "ButtonTriggered", (parser, x) => x.ButtonTriggered = parser.ParseBoolean() },
                { "HealOnlyOthers", (parser, x) => x.HealOnlyOthers = parser.ParseBoolean() },
                { "UnitHealPulseFX", (parser, x) => x.UnitHealPulseFX = parser.ParseAssetReference() },
                { "AffectsContained", (parser, x) => x.AffectsContained = parser.ParseBoolean() },
                { "NonStackable", (parser, x) => x.NonStackable = parser.ParseBoolean() },
                { "RespawnNearbyHordeMembers", (parser, x) => x.RespawnNearbyHordeMembers = parser.ParseBoolean() },
                { "RespawnFXList", (parser, x) => x.RespawnFXList = parser.ParseAssetReference() },
                { "RespawnMinimumDelay", (parser, x) => x.RespawnMinimumDelay = parser.ParseInteger() },
                { "HealOnlyIfNotUnderAttack", (parser, x) => x.HealOnlyIfNotUnderAttack = parser.ParseBoolean() }
            });

        public UpgradeLogicData UpgradeData { get; } = new();

        /// <summary>
        /// Amount to heal with each <see cref="HealingDelay"/>.
        /// </summary>
        public float HealingAmount { get; private set; }

        /// <summary>
        /// Time to wait between successive <see cref="HealingAmount"/> applications.
        /// </summary>
        public LogicFrameSpan HealingDelay { get; private set; }

        /// <summary>
        /// Whether healing should affect the entire player (e.g. true for TechHospital).
        /// </summary>
        public bool AffectsWholePlayer { get; private set; }

        /// <summary>
        /// Object kinds which should be healed by this unit.
        /// </summary>
        public BitArray<ObjectKinds> KindOf { get; private set; }

        /// <summary>
        /// Object kinds which should not be healed by this unit.
        /// </summary>
        public BitArray<ObjectKinds> ForbiddenKindOf { get; private set; }

        /// <summary>
        /// Time to wait after taking damage before beginning healing.
        /// </summary>
        public LogicFrameSpan StartHealingDelay { get; private set; }

        /// <summary>
        /// Radius around gameobject where units should be healed.
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Whether healing should be applied all at once (e.g. true for Emergency Repair Special Power).
        /// </summary>
        public bool SingleBurst { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool SkipSelfForHealing { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HealOnlyIfNotInCombat { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ButtonTriggered { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HealOnlyOthers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string UnitHealPulseFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AffectsContained { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NonStackable { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool RespawnNearbyHordeMembers { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string RespawnFXList { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int RespawnMinimumDelay { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool HealOnlyIfNotUnderAttack { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new AutoHealBehavior(gameObject, this);
        }
    }
}
