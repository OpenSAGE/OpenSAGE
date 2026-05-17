using System;
using OpenSage;
using OpenSage.Content;
using OpenSage.Logic;
using OpenSage.Logic.Orders;
using OpenSage.Tools.ReplaySketch.Model;

namespace OpenSage.Tools.ReplaySketch.Services;

/// <summary>
/// Subscribes to low-level <see cref="IGame"/> events and re-raises them as semantic
/// <see cref="ActionType"/>-keyed notifications that the test harness can react to.
/// </summary>
/// <remarks>
/// "Dispatched" events fire when the replay order for an action is processed.
/// "Completed" events fire when the resulting game effect is observed (e.g. a unit exits production).
/// </remarks>
public sealed class ReplayActionMonitor : IDisposable
{
    private readonly IGame _game;

    /// <summary>
    /// Raised when a replay order corresponding to a <see cref="ActionType"/> is processed.
    /// </summary>
    public event EventHandler<ReplayActionEventArgs>? ActionDispatched;

    /// <summary>
    /// Raised when the game effect of a <see cref="ActionType"/> is observed in game state
    /// (e.g. a recruited unit exits the barracks).
    /// </summary>
    public event EventHandler<ReplayActionEventArgs>? ActionCompleted;

    public ReplayActionMonitor(IGame game)
    {
        _game = game;
        _game.OrderProcessed += OnOrderProcessed;
        _game.ObjectCreated += OnObjectCreated;
    }

    private void OnOrderProcessed(object? sender, OrderProcessedEventArgs e)
    {
        var actionType = e.Order.OrderType switch
        {
            OrderType.BuildObject => ActionType.BuildBarracks,
            OrderType.GatherDumpSupplies => ActionType.GatherResources,
            OrderType.CreateUnit => ActionType.RecruitBasicUnit,
            OrderType.ForceAttackGround => ActionType.AttackEnemyBase,
            _ => (ActionType?)null,
        };

        if (actionType is not null)
        {
            ActionDispatched?.Invoke(this, new ReplayActionEventArgs(actionType.Value, e.Player));
        }
    }

    private void OnObjectCreated(object? sender, GameObjectCreatedEventArgs e)
    {
        var name = e.GameObject.Definition.Name;
        if (name != KnownDefinitions.UsaRangerName && name != KnownDefinitions.GlaRebelName)
        {
            return;
        }

        ActionCompleted?.Invoke(this,
            new ReplayActionEventArgs(ActionType.RecruitBasicUnit, e.GameObject.Owner));
    }

    public void Dispose()
    {
        _game.OrderProcessed -= OnOrderProcessed;
        _game.ObjectCreated -= OnObjectCreated;
    }
}

public sealed class ReplayActionEventArgs : EventArgs
{
    public ActionType ActionType { get; }
    public Player? Player { get; }

    public ReplayActionEventArgs(ActionType actionType, Player? player)
    {
        ActionType = actionType;
        Player = player;
    }
}
