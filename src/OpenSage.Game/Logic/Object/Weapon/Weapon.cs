using System;
using System.Numerics;
using ImGuiNET;

namespace OpenSage.Logic.Object
{
    internal sealed class Weapon
    {
        private readonly ModelConditionFlag _usingFlag;

        private readonly WeaponStateMachine _stateMachine;

        private int _currentRounds;

        public readonly int WeaponIndex;

        public readonly GameObject ParentGameObject;

        public readonly WeaponTemplate Template;

        public int CurrentRounds
        {
            get => _currentRounds;
            internal set => _currentRounds = value;
        }

        public WeaponTarget CurrentTarget { get; private set; }

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
                    ParentGameObject.Transform.Translation);

                return distance <= Template.AttackRange;
            }
        }

        public readonly WeaponSlot Slot;

        internal Weapon(
            GameObject gameObject,
            WeaponTemplate weaponTemplate,
            int weaponIndex,
            WeaponSlot slot,
            GameContext gameContext)
        {
            ParentGameObject = gameObject;
            Template = weaponTemplate;
            WeaponIndex = weaponIndex;

            Slot = slot;

            FillClip();

            _stateMachine = new WeaponStateMachine(
                new WeaponStateContext(
                    gameObject,
                    this,
                    gameContext));

            _usingFlag = ModelConditionFlagUtility.GetUsingWeaponFlag(weaponIndex);
        }

        public bool UsesClip => Template.ClipSize > 0;

        public bool IsClipEmpty() => UsesClip && CurrentRounds == 0;

        public void FillClip()
        {
            CurrentRounds = Template.ClipSize;
        }

        public void SetTarget(WeaponTarget target)
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

        public void LogicTick(TimeInterval time)
        {
            _stateMachine.Update(time);

            if (CurrentTarget != null && CurrentTarget.IsDestroyed)
            {
                CurrentTarget = null;
            }
        }

        public void Fire(TimeInterval time)
        {
            _stateMachine.Fire(time);
        }

        internal void DrawInspector()
        {
            // TODO: Weapon template

            ImGui.InputInt("CurrentRounds", ref _currentRounds);

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
