using System;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class HackInternetState : State
    {
        public const uint StateId = 1001;

        private readonly HackInternetAIUpdate _aiUpdate;

        private LogicFrameSpan _framesUntilNextHack;

        public HackInternetState(GameObject gameObject, GameContext context, HackInternetAIUpdate aiUpdate) : base(gameObject, context)
        {
            _aiUpdate = aiUpdate;
        }

        public override void OnEnter()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, true);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);

            SetFramesUntilNextHack(GameObject);
        }

        public override UpdateStateResult Update()
        {
            if (_framesUntilNextHack-- == LogicFrameSpan.Zero)
            {
                SetFramesUntilNextHack(GameObject);

                Context.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitCashPing?.Value);

                var amount = GetCashGrant(GameObject);

                GameObject.Owner.BankAccount.Deposit((uint)amount);

                GameObject.ActiveCashEvent = new CashEvent(amount, new ColorRgb(0, 255, 0), new Vector3(0, 0, 20));

                GameObject.GainExperience(_aiUpdate.ModuleData.XpPerCashUpdate);
            }

            return UpdateStateResult.Continue();
        }

        public override void OnExit()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
        }

        private void SetFramesUntilNextHack(GameObject gameObject)
        {
            _framesUntilNextHack = gameObject.ContainerId != 0
                ? _aiUpdate.ModuleData.CashUpdateDelayFast
                : _aiUpdate.ModuleData.CashUpdateDelay;
        }

        private int GetCashGrant(GameObject gameObject) => gameObject.Rank switch
        {
            0 => _aiUpdate.ModuleData.RegularCashAmount,
            1 => _aiUpdate.ModuleData.VeteranCashAmount,
            2 => _aiUpdate.ModuleData.EliteCashAmount,
            3 => _aiUpdate.ModuleData.HeroicCashAmount,
            _ => throw new ArgumentOutOfRangeException(nameof(GameObject.Rank)),
        };

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref _framesUntilNextHack);
        }
    }
}
