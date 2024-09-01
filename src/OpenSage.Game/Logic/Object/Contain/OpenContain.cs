#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FixedMath.NET;
using ImGuiNET;
using OpenSage.Audio;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class OpenContainModule : UpdateModule, IHasRallyPoint
    {
        public RallyPointManager RallyPointManager { get; } = new();

        private readonly OpenContainModuleData _moduleData;
        protected GameObject GameObject { get; }
        private protected GameContext GameContext { get; }

        private readonly List<uint> _containedObjectIds = new();
        private uint _unknownFrame1;
        private uint _unknownFrame2;
        private uint _unknownInt2;
        private BitArray<ModelConditionFlag> _cachedModelConditionFlags = new();
        private readonly Matrix4x3[] _unknownTransforms = new Matrix4x3[32];
        private uint _nextFirePointIndex;
        private uint _numFirePoints;
        private bool _hasNoFirePoints;
        private readonly List<QueuedForEvac> _evacQueue = new();
        private int _unknownInt;
        private bool _passengersAllowedToFire;

        public virtual IList<uint> ContainedObjectIds => _containedObjectIds;
        public bool DrawPips => _moduleData.ShouldDrawPips;
        public virtual int TotalSlots => _moduleData.ContainMax;
        public int OccupiedSlots => ContainedObjectIds.Sum(id => SlotValueForUnit(GameObjectForId(id)));
        public bool Full => OccupiedSlots >= TotalSlots;

        protected const string ExitBoneStartName = "ExitStart";
        protected const string ExitBoneEndName = "ExitEnd";

        private protected OpenContainModule(GameObject gameObject, GameContext gameContext, OpenContainModuleData moduleData)
        {
            _moduleData = moduleData;
            GameObject = gameObject;
            GameContext = gameContext;
        }

        public bool CanAddUnit(GameObject unit)
        {
            return unit != GameObject &&
                   !HealthTooLowToHoldUnits() &&
                   _moduleData.ForbidInsideKindOf?.Intersects(unit.Definition.KindOf) != true &&
                   _moduleData.AllowInsideKindOf?.Intersects(unit.Definition.KindOf) == true &&
                   SlotValueForUnit(unit) <= TotalSlots - OccupiedSlots &&
                   !GameObject.IsBeingConstructed() &&
                   CanUnitEnter(unit);
        }

        /// <summary>
        /// Used to allow containers to define additional restrictions.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        protected virtual bool CanUnitEnter(GameObject unit) => true;

        public virtual int SlotValueForUnit(GameObject unit)
        {
            return 1;
        }

        public void Add(GameObject unit, bool initial = false)
        {
            if (!CanAddUnit(unit))
            {
                return;
            }

            ContainedObjectIds.Add(unit.ID);
            unit.AddToContainer(GameObject.ID);
            if (!initial)
            {
                GameObject.GameContext.AudioSystem.PlayAudioEvent(unit, GetEnterVoiceLine(unit.Definition.UnitSpecificSounds));
            }
        }

        public (Vector3? Start, Vector3? End) DefaultExitPath => (
            GameObject.Drawable.FindBone(ExitBoneStartName).bone?.Transform.Translation,
            GameObject.Drawable.FindBone(ExitBoneEndName).bone?.Transform.Translation);

        protected virtual BaseAudioEventInfo? GetEnterVoiceLine(UnitSpecificSounds sounds)
        {
            return sounds.VoiceEnter?.Value;
        }

        public void Remove(uint unitId)
        {
            if (_evacQueue.Any(u => u.ObjectId == unitId))
            {
                return; // this unit is already queued for evac, nothing to do here
            }

            _evacQueue.Add(new QueuedForEvac { ObjectId = unitId });
        }

        public void Evacuate()
        {
            GameObject.GameContext.AudioSystem.PlayAudioEvent(GameObject,
                GameObject.Definition.UnitSpecificSounds.VoiceUnload?.Value);
            foreach (var id in ContainedObjectIds)
            {
                Remove(id);
            }
        }

        protected virtual bool HealthTooLowToHoldUnits()
        {
            return GameObject.HealthPercentage <= Fix64.Zero;
        }

        internal sealed override void Update(BehaviorUpdateContext context)
        {
            UpdateModuleSpecific(context);

            if (HealthTooLowToHoldUnits())
            {
                foreach (var unitId in ContainedObjectIds.ToArray()) // we're modifying the collection, so we need a copy of it
                {
                    RemoveUnit(unitId, true);
                }

                _evacQueue.Clear();
            }
            else
            {
                while (_evacQueue.Count > 0 && TryEvacUnit(context.LogicFrame, _evacQueue[0].ObjectId))
                {
                    _evacQueue.RemoveAt(0);
                }
            }
        }

        private protected virtual void UpdateModuleSpecific(BehaviorUpdateContext context) { }

        protected virtual bool TryEvacUnit(LogicFrame currentFrame, uint unitId)
        {
            RemoveUnit(unitId);
            return true;
        }

        protected virtual bool TryAssignExitPath(GameObject unit)
        {
            return false;
        }

        protected void RemoveUnit(uint unitId, bool exitDueToParentDeath = false)
        {
            var unit = GameObjectForId(unitId);

            ContainedObjectIds.Remove(unitId);

            if (exitDueToParentDeath || !TryAssignExitPath(unit))
            {
                unit.UpdateTransform(GameObject.Transform.Translation, GameObject.Transform.Rotation);
            }

            if (exitDueToParentDeath)
            {
                if (!_moduleData.DamagePercentToUnits.IsZero)
                {
                    // this is dealt when the parent dies
                    var damageToDeal = unit.MaxHealth * (Fix64)(float)_moduleData.DamagePercentToUnits;
                    // yes, the container is the damager, not the one who destroyed the container
                    // and yes, the death type is in fact burned
                    unit.DoDamage(DamageType.Unresistable, damageToDeal, DeathType.Burned, GameObject);
                }
            }
            else
            {
                GameObject.GameContext.AudioSystem.PlayAudioEvent(GameObject.Definition.SoundExit?.Value);
            }

            unit.RemoveFromContainer();
        }

        protected void HealUnits(int fullHealTimeMs)
        {
            var percentToHeal = new Percentage(1 / (Game.LogicFramesPerSecond * (fullHealTimeMs / 1000f)));
            foreach (var unitId in ContainedObjectIds)
            {
                var unit = GameObjectForId(unitId);
                unit.Heal(percentToHeal, GameObject);
            }
        }

        protected GameObject GameObjectForId(uint unitId)
        {
            return GameObject.GameContext.GameLogic.GetObjectById(unitId);
        }

        internal override void Load(StatePersister reader)
        {
            var version = reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistListWithUInt32Count(
                _containedObjectIds,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistObjectIDValue(ref item);
                });

            reader.SkipUnknownBytes(2);

            reader.PersistFrame(ref _unknownFrame1);
            reader.PersistFrame(ref _unknownFrame2);

            reader.SkipUnknownBytes(4);

            reader.PersistUInt32(ref _unknownInt2);

            reader.PersistBitArray(ref _cachedModelConditionFlags);

            // Where does the 32 come from?
            reader.PersistArray(
                _unknownTransforms,
                static (StatePersister persister, ref Matrix4x3 item) =>
                {
                    persister.PersistMatrix4x3Value(ref item, readVersion: false);
                });

            var unknown6 = -1;
            reader.PersistInt32(ref unknown6);
            if (unknown6 != -1)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32(ref _nextFirePointIndex);
            reader.PersistUInt32(ref _numFirePoints);
            reader.PersistBoolean(ref _hasNoFirePoints);

            reader.PersistObject(RallyPointManager);

            reader.PersistList(
                _evacQueue,
                static (StatePersister persister, ref QueuedForEvac item) =>
                {
                    persister.PersistObjectValue(ref item);
                });

            reader.PersistInt32(ref _unknownInt);

            if (version >= 2)
            {
                reader.PersistBoolean(ref _passengersAllowedToFire);
            }
        }

        internal override void DrawInspector()
        {
            if (ImGui.Button("Evacuate"))
            {
                Evacuate();
            }
        }

        private struct QueuedForEvac : IPersistableObject
        {
            public uint ObjectId;
            public int Unknown;

            public void Persist(StatePersister persister)
            {
                persister.PersistObjectID(ref ObjectId);
                persister.PersistInt32(ref Unknown); // todo: is this version?
            }
        }
    }

    public abstract class OpenContainModuleData : UpdateModuleData
    {
        internal static readonly IniParseTable<OpenContainModuleData> FieldParseTable = new IniParseTable<OpenContainModuleData>
        {
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ForbidInsideKindOf", (parser, x) => x.ForbidInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
            { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "PassengersInTurret", (parser, x) => x.PassengersInTurret = parser.ParseBoolean() },
            { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
            { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
            { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() },
            { "ShouldDrawPips", (parser, x) => x.ShouldDrawPips = parser.ParseBoolean() },
        };

        public virtual BitArray<ObjectKinds> AllowInsideKindOf { get; protected set; } = new();
        public BitArray<ObjectKinds>? ForbidInsideKindOf { get; private set; }
        public int ContainMax { get; private set; }
        public string? EnterSound { get; private set; }
        public string? ExitSound { get; private set; }
        public Percentage DamagePercentToUnits { get; private set; }
        public bool PassengersInTurret { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
        public bool ShouldDrawPips { get; private set; } = true;
    }
}
