#nullable enable

using System;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class HackInternetState : State
{
    public const uint StateId = 1001;

    private readonly HackInternetAIUpdateStateMachine _stateMachine;

    private LogicFrameSpan _framesUntilNextHack;

    public HackInternetState(HackInternetAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = stateMachine;
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

            GameEngine.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitCashPing?.Value);

            var amount = GetCashGrant(GameObject);

            GameObject.Owner.BankAccount.Deposit((uint)amount);

            GameObject.ActiveCashEvent = new CashEvent(amount, new ColorRgb(0, 255, 0), new Vector3(0, 0, 20));

            GameObject.ExperienceTracker.AddExperiencePoints(_stateMachine.AIUpdate.ModuleData.XpPerCashUpdate);
        }

        return UpdateStateResult.Continue();
    }

    public override void OnExit()
    {
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
    }

    private void SetFramesUntilNextHack(GameObject gameObject)
    {
        _framesUntilNextHack = gameObject.ContainerId.IsValid
            ? _stateMachine.AIUpdate.ModuleData.CashUpdateDelayFast
            : _stateMachine.AIUpdate.ModuleData.CashUpdateDelay;
    }

    private int GetCashGrant(GameObject gameObject) => gameObject.Rank switch
    {
        VeterancyLevel.Regular => _stateMachine.AIUpdate.ModuleData.RegularCashAmount,
        VeterancyLevel.Veteran => _stateMachine.AIUpdate.ModuleData.VeteranCashAmount,
        VeterancyLevel.Elite => _stateMachine.AIUpdate.ModuleData.EliteCashAmount,
        VeterancyLevel.Heroic => _stateMachine.AIUpdate.ModuleData.HeroicCashAmount,
        _ => throw new ArgumentOutOfRangeException(nameof(GameObject.Rank)),
    };

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistLogicFrameSpan(ref _framesUntilNextHack);
    }
}
