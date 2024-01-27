﻿using System.Numerics;
using ImGuiNET;

namespace OpenSage.Logic.Object
{
    public sealed class Weapon : IPersistableObject
    {
        private readonly ModelConditionFlag _usingFlag;

        private readonly WeaponStateMachine _stateMachine;

        private int _currentRounds;

        private uint _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private uint _unknownFrame1;
        private uint _unknownFrame2;
        private uint _unknownFrame3;
        private uint _unknownFrame4;
        private uint _unknownObjectId;
        private uint _unknownInt4;
        private uint _unknownInt5;
        private uint _unknownInt6;
        private bool _unknownBool1;
        private bool _unknownBool2;

        public readonly int WeaponIndex;

        public readonly GameObject ParentGameObject;

        public readonly WeaponTemplate Template;

        public int CurrentRounds
        {
            get => _currentRounds;
            internal set => _currentRounds = value;
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

        public readonly WeaponSlot Slot;

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

            FillClip();

            _stateMachine = new WeaponStateMachine(
                new WeaponStateContext(
                    gameObject,
                    this,
                    gameContext));

            _usingFlag = ModelConditionFlagUtility.GetUsingWeaponFlag(WeaponIndex);
        }

        public bool UsesClip => Template.ClipSize > 0;

        public bool IsClipEmpty() => UsesClip && CurrentRounds == 0;

        public void FillClip()
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

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistFrame(ref _unknownFrame1);

            reader.SkipUnknownBytes(4);

            reader.PersistFrame(ref _unknownFrame2);
            reader.PersistFrame(ref _unknownFrame3);
            reader.PersistFrame(ref _unknownFrame4);
            reader.PersistObjectID(ref _unknownObjectId);

            reader.SkipUnknownBytes(4);

            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _unknownInt5);
            reader.PersistUInt32(ref _unknownInt6);

            reader.SkipUnknownBytes(2);

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
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
