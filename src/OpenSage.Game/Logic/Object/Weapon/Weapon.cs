using System.Numerics;
using ImGuiNET;

namespace OpenSage.Logic.Object
{
    public sealed class Weapon : IPersistableObject
    {
        private readonly ModelConditionFlag _usingFlag;

        private readonly WeaponStateMachine _stateMachine;

        private WeaponStatus _status;
        private int _shotsLeftInClip;
        private LogicFrame _nextFrameToFire;
        private LogicFrame _preAttackCompleteFrame;
        private LogicFrame _lastFrameFired1;
        private LogicFrame _lastFrameFired2;
        private LogicFrame _unknownFrame;
        private uint _unknownObjectId;
        private int _maxShots;
        private uint _unknownInt1;
        private uint _unknownInt2;
        private bool _unknownBool1;
        private bool _isLeechRangeWeapon;

        public readonly int WeaponIndex;

        public readonly GameObject ParentGameObject;

        public readonly WeaponTemplate Template;

        public int CurrentRounds
        {
            get => _shotsLeftInClip;
            internal set => _shotsLeftInClip = value;
        }

        internal WeaponTarget CurrentTarget { get; private set; }

        private Vector3? CurrentTargetPosition => CurrentTarget?.TargetPosition;

        public bool HasValidTarget
        {
            get
            {
                var currentTargetPosition = CurrentTargetPosition;
                if (currentTargetPosition == null)
                {
                    return false;
                }

                var distance = Vector3.Distance(
                    currentTargetPosition.Value,
                    ParentGameObject.Translation);

                return distance <= Template.AttackRange;
            }
        }

        public bool IsInactive => _stateMachine.IsInactive;

        public readonly WeaponSlot Slot;

        internal WeaponStatus Status => _status;
        internal int ShotsLeftInClip => _shotsLeftInClip;
        internal LogicFrame NextFrameToFire => _nextFrameToFire;
        internal LogicFrame LastFrameFired1 => _lastFrameFired1;
        internal LogicFrame LastFrameFired2 => _lastFrameFired2;
        internal LogicFrame UnknownFrame => _unknownFrame;
        internal uint UnknownObjectId => _unknownObjectId;
        internal int MaxShots => _maxShots;
        internal uint UnknownInt1 => _unknownInt1;
        internal uint UnknownInt2 => _unknownInt2;
        internal bool UnknownBool1 => _unknownBool1;
        internal bool UnknownBool2 => _isLeechRangeWeapon;

        internal Weapon(
            GameObject gameObject,
            WeaponTemplate weaponTemplate,
            WeaponSlot slot,
            GameContext gameContext)
        {
            ParentGameObject = gameObject;
            Template = weaponTemplate;

            WeaponIndex = (int)slot;

            Slot = slot;

            _maxShots = 0x7FFFFFFF;

            Reload();

            _stateMachine = new WeaponStateMachine(
                new WeaponStateContext(
                    gameObject,
                    this,
                    gameContext));

            _usingFlag = ModelConditionFlagUtility.GetUsingWeaponFlag(WeaponIndex);
        }

        public bool UsesClip => Template.ClipSize > 0;

        public bool IsClipEmpty() => UsesClip && CurrentRounds == 0;

        public void Reload()
        {
            CurrentRounds = Template.ClipSize;
        }

        internal void SetTarget(WeaponTarget target)
        {
            if (CurrentTarget == target)
            {
                return;
            }

            if (CurrentTarget != null)
            {
                ParentGameObject.ModelConditionFlags.Set(_usingFlag, false);
            }

            CurrentTarget = target;

            if (CurrentTarget != null)
            {
                ParentGameObject.ModelConditionFlags.Set(_usingFlag, true);
            }
        }

        public void LogicTick()
        {
            _stateMachine.Update();

            if (CurrentTarget != null && CurrentTarget.IsDestroyed)
            {
                CurrentTarget = null;
            }
        }

        public void Fire()
        {
            _stateMachine.Fire();
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(3);

            var templateName = Template.Name;
            reader.PersistAsciiString(ref templateName);
            if (templateName != Template.Name)
            {
                throw new InvalidStateException();
            }

            var slot = Slot;
            reader.PersistEnum(ref slot);
            if (slot != Slot)
            {
                throw new InvalidStateException();
            }

            reader.PersistEnum(ref _status);
            reader.PersistInt32(ref _shotsLeftInClip);
            reader.PersistLogicFrame(ref _nextFrameToFire);
            reader.PersistLogicFrame(ref _preAttackCompleteFrame);
            reader.PersistLogicFrame(ref _lastFrameFired1);
            reader.PersistLogicFrame(ref _lastFrameFired2);
            reader.PersistLogicFrame(ref _unknownFrame);
            reader.PersistObjectID(ref _unknownObjectId);

            reader.SkipUnknownBytes(4);

            reader.PersistInt32(ref _maxShots);
            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);

            reader.SkipUnknownBytes(2);

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _isLeechRangeWeapon);
        }

        internal void DrawInspector()
        {
            // TODO: Weapon template

            ImGui.InputInt("ShotsLeftInClip", ref _shotsLeftInClip);

            ImGui.LabelText("CurrentTarget", CurrentTarget?.TargetType.ToString() ?? "[none]");

            if (CurrentTarget != null && CurrentTarget.TargetType == WeaponTargetType.Position)
            {
                var currentTargetPosition = CurrentTarget.TargetPosition;
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
                ImGui.InputFloat3("CurrentTargetPosition", ref currentTargetPosition);
                ImGui.PopStyleVar();
            }
        }
    }
}
