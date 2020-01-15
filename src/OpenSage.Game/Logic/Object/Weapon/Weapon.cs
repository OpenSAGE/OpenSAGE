using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenSage.Logic.Object
{
    internal sealed class Weapon
    {
        private readonly GameObject _gameObject;
        private readonly WeaponTemplate _weaponTemplate;
        private readonly int _weaponIndex;

        private readonly WeaponStateMachine _stateMachine;

        public readonly WeaponEffectNugget[] Nuggets;

        public int CurrentRounds { get; private set; }

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
                    _gameObject.Transform.Translation);

                return distance <= _weaponTemplate.AttackRange;
            }
        }

        internal Weapon(GameObject gameObject, WeaponTemplate weaponTemplate, int weaponIndex)
        {
            _gameObject = gameObject;
            _weaponTemplate = weaponTemplate;
            _weaponIndex = weaponIndex;

            FillClip();

            var nuggets = new List<WeaponEffectNugget>();
            foreach (var nuggetData in weaponTemplate.Nuggets)
            {
                var nugget = nuggetData.CreateNugget(this);
                if (nugget != null) // TODO: This should never be null.
                {
                    nuggets.Add(nugget);
                }
            }
            Nuggets = nuggets.ToArray();

            _stateMachine = new WeaponStateMachine(
                new WeaponStateContext(
                    gameObject,
                    this,
                    weaponTemplate,
                    weaponIndex));
        }

        public bool IsClipEmpty() => _weaponTemplate.ClipSize > 0 && CurrentRounds == 0;

        public void FillClip()
        {
            CurrentRounds = _weaponTemplate.ClipSize;
        }

        public void SetTarget(WeaponTarget target)
        {
            if (CurrentTarget == target)
            {
                return;
            }

            CurrentTarget = target;
        }

        public void LogicTick(TimeSpan currentTime)
        {
            _stateMachine.Update(currentTime);
        }
    }
}
