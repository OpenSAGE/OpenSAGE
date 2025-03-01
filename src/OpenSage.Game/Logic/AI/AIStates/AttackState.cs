using System;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class AttackState(StateMachineBase stateMachine) : State
    {
        private readonly AttackStateMachine _attackStateMachine = new();

        private bool _unknownBool;
        private Vector3 _unknownPosition;

        public override void OnEnter()
        {
            stateMachine.GameObject.SetObjectStatus(ObjectStatus.IsAttacking, true);
            stateMachine.GameObject.ModelConditionFlags.Set(ModelConditionFlag.Attacking, true);

            // TODO: set _attackStateMachine.TargetObject and _attackStateMachine.TargetPosition
            // based on our parent StateMachine.
        }

        public override void OnExit()
        {
            stateMachine.GameObject.ModelConditionFlags.Set(ModelConditionFlag.Attacking, false);
            stateMachine.GameObject.SetObjectStatus(ObjectStatus.IsAttacking, false);
        }

        public override UpdateStateResult Update()
        {
            // TODO: Return result from _stateMachine.Update() ?
            _attackStateMachine.Update();

            return UpdateStateResult.Continue();
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBoolean(ref _unknownBool);
            reader.PersistVector3(ref _unknownPosition);
            reader.PersistObject(_attackStateMachine);
        }
    }

    internal sealed class AttackStateMachine : StateMachineBase
    {
        private const uint AimWeaponStateId = 2;
        private const uint FireWeaponStateId = 3;

        public AttackStateMachine()
        {
            AddState(0, new AttackMoveTowardsTargetState());
            AddState(1, new AttackMoveTowardsTargetState());
            AddState(AimWeaponStateId, new AttackAimWeaponState(this));
            AddState(FireWeaponStateId, new AttackFireWeaponState(this));
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();
        }

        private sealed class AttackMoveTowardsTargetState : MoveTowardsState
        {
            private Vector3 _unknownPosition;
            private uint _unknownFrame;
            private bool _unknownBool1;
            private bool _unknownBool2;
            private bool _unknownBool3;
            private bool _unknownBool4;

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.BeginObject("Base");
                base.Persist(reader);
                reader.EndObject();

                reader.PersistVector3(ref _unknownPosition);
                reader.PersistFrame(ref _unknownFrame);
                reader.PersistBoolean(ref _unknownBool1);
                reader.PersistBoolean(ref _unknownBool2);
                reader.PersistBoolean(ref _unknownBool3);
                reader.PersistBoolean(ref _unknownBool4);
            }
        }

        private sealed class AttackAimWeaponState(StateMachineBase stateMachine) : State
        {
            private bool _unknownBool1;
            private bool _unknownBool2;

            public override void OnEnter()
            {
                stateMachine.GameObject.SetObjectStatus(ObjectStatus.IsAimingWeapon, true);
            }

            public override void OnExit()
            {
                stateMachine.GameObject.SetObjectStatus(ObjectStatus.IsAimingWeapon, false);
            }

            public override UpdateStateResult Update()
            {
                var acceptableAimDelta = stateMachine.GameObject.CurrentWeapon.Template.AcceptableAimDelta;
                var angleToTarget = stateMachine.GameObject.GetAngleTo(stateMachine.TargetObject.Translation);

                if (angleToTarget > acceptableAimDelta)
                {
                    stateMachine.GameObject.AIUpdate.SetTargetAngle(stateMachine.GameObject.Yaw + angleToTarget);
                    return UpdateStateResult.Continue();
                }
                
                return UpdateStateResult.TransitionToState(FireWeaponStateId);
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistBoolean(ref _unknownBool1);
                reader.PersistBoolean(ref _unknownBool2);
            }
        }

        private sealed class AttackFireWeaponState(StateMachineBase stateMachine) : State
        {
            public override void OnEnter()
            {
                stateMachine.GameObject.SetObjectStatus(ObjectStatus.IsFiringWeapon, true);
            }

            public override void OnExit()
            {
                stateMachine.GameObject.SetObjectStatus(ObjectStatus.IsFiringWeapon, false);
            }

            public override UpdateStateResult Update()
            {
                var weapon = stateMachine.GameObject.CurrentWeapon;

                switch (weapon.Status)
                {
                    case WeaponStatus.PreAttack:
                        return UpdateStateResult.Continue();

                    case WeaponStatus.Ready:
                        weapon.Fire();
                        return UpdateStateResult.TransitionToState(AimWeaponStateId);

                    default:
                        throw new InvalidOperationException();
                }
            }

            public override void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);
            }
        }
    }
}
