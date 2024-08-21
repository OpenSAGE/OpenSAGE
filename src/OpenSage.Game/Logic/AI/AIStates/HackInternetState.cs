using System;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class HackInternetState : State
    {
        public const uint StateId = 1001;

        private readonly GameObject _gameObject;

        private LogicFrameSpan _framesUntilNextHack;

        private HackInternetAIUpdateModuleData ModuleData => ((HackInternetAIUpdate)_gameObject.AIUpdate).ModuleData;

        public HackInternetState(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public override void OnEnter()
        {
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, true);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);

            SetFramesUntilNextHack();
        }

        public override UpdateStateResult Update()
        {
            if (_framesUntilNextHack-- == LogicFrameSpan.Zero)
            {
                SetFramesUntilNextHack();

                _gameObject.GameContext.AudioSystem.PlayAudioEvent(_gameObject, _gameObject.Definition.UnitSpecificSounds.UnitCashPing?.Value);

                var amount = GetCashGrant();

                _gameObject.Owner.BankAccount.Deposit((uint)amount);

                _gameObject.ActiveCashEvent = new CashEvent(amount, new ColorRgb(0, 255, 0), new Vector3(0, 0, 20));

                _gameObject.GainExperience(ModuleData.XpPerCashUpdate);
            }

            return UpdateStateResult.Continue();
        }

        public override void OnExit()
        {
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
        }

        private void SetFramesUntilNextHack()
        {
            _framesUntilNextHack = _gameObject.ContainerId != 0
                ? ModuleData.CashUpdateDelayFast
                : ModuleData.CashUpdateDelay;
        }

        private int GetCashGrant() => _gameObject.Rank switch
        {
            0 => ModuleData.RegularCashAmount,
            1 => ModuleData.VeteranCashAmount,
            2 => ModuleData.EliteCashAmount,
            3 => ModuleData.HeroicCashAmount,
            _ => throw new ArgumentOutOfRangeException(nameof(GameObject.Rank)),
        };

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref _framesUntilNextHack);
        }
    }
}
