﻿using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.AI;
using OpenSage.Logic.AI.AIStates;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class JetAIUpdate : AIUpdate
{
    public GameObject Base;
    internal override JetAIUpdateModuleData ModuleData { get; }

    private Vector3 _currentTargetPoint;
    private bool _unparkingRequested;

    private Queue<string> _pathToStart;
    private Queue<string> _pathToParking;
    private string _currentTaxiingTarget;

    public JetAIState CurrentJetAIState;

    private LogicFrame _waitUntil;

    private bool _afterburnerEnabled;

    private Vector3 _positionSomething;

    private uint _unknownInt6;
    private uint _unknownInt7;
    private uint _unknownInt8;
    private uint _unknownInt9;
    private uint _unknownInt10;
    private uint _unknownInt11;
    private bool _unknownBool5;

    private readonly UnknownStateData _stateData = new();

    public enum JetAIState
    {
        JustCreated, //TODO: better name
        MovingToParkingPlace,
        ReachedParkingPlace,
        Rotating, //TODO: better name
        Parked,
        MovingTowardsStart,
        PreparingStart,
        Starting,
        Started,
        MovingTowardsTarget,
        ReachedTargetPoint,
        Landing,
        ReturningToBase,
        MovingBackToHangar
    }

    internal JetAIUpdate(GameObject gameObject, IGameEngine gameEngine, JetAIUpdateModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        ModuleData = moduleData;
        CurrentJetAIState = JetAIState.Parked;
    }

    private protected override JetAIUpdateStateMachine CreateStateMachine() => new(GameObject, GameEngine, this);

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(2);
        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistVector3(ref _positionSomething);

        reader.PersistObject(_stateData);

        reader.PersistUInt32(ref _unknownInt6); // 255, probably frameSomething
        reader.PersistUInt32(ref _unknownInt7); // 255, probably frameSomething
        reader.PersistUInt32(ref _unknownInt8); // 0
        reader.PersistUInt32(ref _unknownInt9); // 1
        reader.PersistUInt32(ref _unknownInt10); // 0
        reader.PersistUInt32(ref _unknownInt11); // 6

        reader.PersistBoolean(ref _unknownBool5);
    }

    internal override void SetTargetPoint(Vector3 targetPoint)
    {
        if (ModuleData.KeepsParkingSpaceWhenAirborne) // check if not a helicopter
        {
            switch (CurrentJetAIState)
            {
                case JetAIState.JustCreated:
                case JetAIState.MovingToParkingPlace:
                case JetAIState.ReachedParkingPlace:
                case JetAIState.Rotating:
                case JetAIState.Parked:
                case JetAIState.MovingTowardsStart:
                case JetAIState.Starting:
                case JetAIState.MovingBackToHangar:
                    _currentTargetPoint = targetPoint;
                    _unparkingRequested = true;
                    return;
                case JetAIState.Started:
                case JetAIState.MovingTowardsTarget:
                case JetAIState.ReachedTargetPoint:
                case JetAIState.ReturningToBase:
                    CurrentJetAIState = JetAIState.MovingTowardsTarget;
                    break;
            }
        }
        base.SetTargetPoint(targetPoint);
    }

    public override UpdateSleepTime Update()
    {
        var sleepTime = base.Update();

        if (!ModuleData.KeepsParkingSpaceWhenAirborne)
        {
            return sleepTime; // helicopters are way more simple (at least for now)
        }

        if (Base == null)
        {
            // fix for mapObject aircrafts etc (e.g. ZH shellmap)
            // TODO: handle scenario of a destroyed airfield
            return sleepTime;
        }

        var parkingPlaceBehavior = Base.FindBehavior<ParkingPlaceBehaviour>();

        var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

        var trans = GameObject.Translation;

        var terrainHeight = GameEngine.Terrain.HeightMap.GetHeight(trans.X, trans.Y);

        switch (CurrentJetAIState)
        {
            case JetAIState.JustCreated:
                var parkingTransform = parkingPlaceBehavior.GetParkingTransform(GameObject.Id);
                var parkingOffset = Vector4.Transform(new Vector4(ModuleData.ParkingOffset, 0, 0, 1),
                    parkingTransform.Rotation).ToVector3();
                base.SetTargetPoint(Base.ToWorldspace(parkingTransform.Translation + parkingOffset));
                CurrentJetAIState = JetAIState.MovingToParkingPlace;
                break;

            case JetAIState.MovingToParkingPlace:
                if (isMoving)
                {
                    break;
                }

                CurrentJetAIState = JetAIState.ReachedParkingPlace;
                break;

            case JetAIState.ReachedParkingPlace:
                var createTransform = Base.ToWorldspace(parkingPlaceBehavior.GetUnitCreateTransform(GameObject.Id));
                SetTargetDirection(createTransform.LookDirection);
                CurrentJetAIState = JetAIState.Rotating;
                break;

            case JetAIState.Rotating:
                if (isMoving)
                {
                    break;
                }

                //base.SetTargetPoint(GameObject.Transform.Translation + GameObject.Transform.LookDirection * _moduleData.ParkingOffset);
                parkingPlaceBehavior.ReportParkedIdle(GameObject.Id, GameEngine.GameLogic.CurrentFrame);
                CurrentJetAIState = JetAIState.Parked;
                break;

            case JetAIState.Parked:
                if (_unparkingRequested)
                {
                    if (!parkingPlaceBehavior.ReportReadyToTaxi(GameObject.Id, out var runway))
                    {
                        break;
                    }
                    _pathToStart = parkingPlaceBehavior.GetPathToRunway(GameObject.Id, runway);
                    CurrentJetAIState = JetAIState.MovingTowardsStart;
                    _unparkingRequested = false;
                }
                break;

            case JetAIState.MovingTowardsStart:
                if (isMoving || ProcessWaypointPath(parkingPlaceBehavior, _pathToStart))
                {
                    break;
                }

                parkingPlaceBehavior.ReportEngineRunUp(GameObject.Id);
                CurrentJetAIState = JetAIState.PreparingStart;
                _waitUntil = GameEngine.GameLogic.CurrentFrame + ModuleData.TakeoffPause;
                break;

            case JetAIState.PreparingStart:
                if (GameEngine.GameLogic.CurrentFrame < _waitUntil)
                {
                    break;
                }

                SetLocomotor(LocomotorSetType.Normal);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetExhaust, true);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, true);
                _afterburnerEnabled = true;
                var endPointPosition =
                    Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject.Id));
                base.SetTargetPoint(endPointPosition);
                AddTargetPoint(_currentTargetPoint);
                CurrentJetAIState = JetAIState.Starting;
                CurrentLocomotor.LiftFactor = 0;
                break;

            case JetAIState.Starting:
                var speedPercentage = GameObject.Speed / CurrentLocomotor.GetMaxSpeedForCondition(GameObject.BodyModule.DamageState);
                CurrentLocomotor.LiftFactor = speedPercentage;

                if (speedPercentage < ModuleData.TakeoffSpeedForMaxLift)
                {
                    break;
                }

                // todo: this actually shouldn't happen until we're above a certain altitude
                parkingPlaceBehavior.ReportDeparted(GameObject.Id);
                CurrentJetAIState = JetAIState.MovingTowardsTarget;
                break;

            case JetAIState.MovingTowardsTarget:
                if (isMoving)
                {
                    if (_afterburnerEnabled && trans.Z - terrainHeight >= CurrentLocomotor.LocomotorTemplate.PreferredHeight)
                    {
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, false);
                        _afterburnerEnabled = false;
                    }
                    break;
                }
                CurrentJetAIState = JetAIState.ReachedTargetPoint;
                _waitUntil = GameEngine.GameLogic.CurrentFrame + ModuleData.ReturnToBaseIdleTime;
                break;

            case JetAIState.ReachedTargetPoint:
                if (GameEngine.GameLogic.CurrentFrame < _waitUntil)
                {
                    break;
                }

                parkingPlaceBehavior.ReportInbound(GameObject.Id);
                var endPosition = Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject.Id));

                base.SetTargetPoint(endPosition);
                CurrentJetAIState = JetAIState.Landing;
                break;
            case JetAIState.Landing:
                parkingPlaceBehavior.ReportLanding(GameObject.Id);
                CurrentJetAIState = JetAIState.ReturningToBase;
                break;

            case JetAIState.ReturningToBase:
                if (isMoving)
                {
                    break;
                }

                GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetExhaust, false);
                CurrentJetAIState = JetAIState.MovingBackToHangar;
                SetLocomotor(LocomotorSetType.Taxiing);
                var (landedRunway, parkingSlot) = parkingPlaceBehavior.ReportLanded(GameObject.Id);
                _pathToParking = parkingPlaceBehavior.GetPathToHangar(landedRunway, parkingSlot);
                break;

            case JetAIState.MovingBackToHangar:
                if (isMoving || ProcessWaypointPath(parkingPlaceBehavior, _pathToParking))
                {
                    break;
                }

                CurrentJetAIState = JetAIState.JustCreated;
                break;

        }

        if (trans.Z - terrainHeight < ModuleData.MinHeight)
        {
            trans.Z = terrainHeight + ModuleData.MinHeight;
            GameObject.SetTranslation(trans);
        }

        if (GameObject.ModelConditionFlags.Get(ModelConditionFlag.Dying))
        {
            Base.ProductionUpdate?.CloseDoor(parkingPlaceBehavior.ClearObjectFromSlot(GameObject.Id));
        }

        return sleepTime;
    }

    private bool ProcessWaypointPath(ParkingPlaceBehaviour parkingPlaceBehavior, Queue<string> path)
    {
        if (_currentTaxiingTarget != null)
        {
            parkingPlaceBehavior.ClearRunway(GameObject.Id);
        }

        if (path.Count > 0)
        {
            var nextPoint = path.Peek();
            if (parkingPlaceBehavior.IsTaxiingPointBlocked(nextPoint))
            {
                _waitUntil = GameEngine.GameLogic.CurrentFrame + ModuleData.TakeoffPause;
                return true;
            }
            if (GameEngine.GameLogic.CurrentFrame < _waitUntil)
            {
                return true;
            }

            _currentTaxiingTarget = nextPoint;
            parkingPlaceBehavior.ReserveRunway(GameObject.Id);
            base.SetTargetPoint(Base.ToWorldspace(parkingPlaceBehavior.GetBoneTranslation(path.Dequeue())));
            return true;
        }

        return false;
    }
}

internal sealed class JetAIUpdateStateMachine : AIUpdateStateMachine
{
    public override JetAIUpdate AIUpdate { get; }

    public JetAIUpdateStateMachine(GameObject gameObject, IGameEngine gameEngine, JetAIUpdate aiUpdate)
        : base(gameObject, gameEngine, aiUpdate)
    {
        AIUpdate = aiUpdate;

        DefineState(JetAIStateIds.CirclingDeadAirfield, new WaitForAirfieldWinchesterState(this), AIStateIds.Idle, AIStateIds.Idle);
    }
}

internal static class JetAIStateIds
{
    public static readonly StateId CirclingDeadAirfield = new(1013);
}

/// <summary>
/// Allows the use of VoiceLowFuel and Afterburner within UnitSpecificSounds section of the object.
/// Requires Kindof = AIRCRAFT.
/// Allows the use of JETEXHAUST JETAFTERBURNER model condition states; this is triggered when
/// it's taking off from the runway.
/// </summary>
public sealed class JetAIUpdateModuleData : AIUpdateModuleData
{
    internal new static JetAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private new static readonly IniParseTable<JetAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<JetAIUpdateModuleData>
        {
            { "OutOfAmmoDamagePerSecond", (parser, x) => x.OutOfAmmoDamagePerSecond = parser.ParsePercentage() },
            { "TakeoffSpeedForMaxLift", (parser, x) => x.TakeoffSpeedForMaxLift = parser.ParsePercentage() },
            { "TakeoffDistForMaxLift", (parser, x) => x.TakeoffDistForMaxLift = parser.ParsePercentage() },
            { "TakeoffPause", (parser, x) => x.TakeoffPause = parser.ParseTimeMillisecondsToLogicFrames() },
            { "MinHeight", (parser, x) => x.MinHeight = parser.ParseInteger() },
            { "NeedsRunway", (parser, x) => x.NeedsRunway = parser.ParseBoolean() },
            { "KeepsParkingSpaceWhenAirborne", (parser, x) => x.KeepsParkingSpaceWhenAirborne = parser.ParseBoolean() },
            { "SneakyOffsetWhenAttacking", (parser, x) => x.SneakyOffsetWhenAttacking = parser.ParseFloat() },
            { "AttackLocomotorType", (parser, x) => x.AttackLocomotorType = parser.ParseEnum<LocomotorSetType>() },
            { "AttackLocomotorPersistTime", (parser, x) => x.AttackLocomotorPersistTime = parser.ParseInteger() },
            { "AttackersMissPersistTime", (parser, x) => x.AttackersMissPersistTime = parser.ParseInteger() },
            { "ReturnForAmmoLocomotorType", (parser, x) => x.ReturnForAmmoLocomotorType = parser.ParseEnum<LocomotorSetType>() },
            { "ParkingOffset", (parser, x) => x.ParkingOffset = parser.ParseInteger() },
            { "ReturnToBaseIdleTime", (parser, x) => x.ReturnToBaseIdleTime = parser.ParseTimeMillisecondsToLogicFrames() },
        });

    /// <summary>
    /// Amount of damage, as a percentage of max health, to take per second when out of ammo.
    /// </summary>
    public Percentage OutOfAmmoDamagePerSecond { get; private set; }
    /// <summary>
    /// smaller numbers give more lift sooner when taking off
    /// </summary>
    public Percentage TakeoffSpeedForMaxLift { get; private set; }
    public LogicFrameSpan TakeoffPause { get; private set; }
    public int MinHeight { get; private set; }

    /// <summary>
    /// comanche (helicopter) does not need a runway
    /// </summary>
    public bool NeedsRunway { get; private set; } = true;

    /// <summary>
    /// comanche (helicopter) does not keep its parking space
    /// </summary>
    public bool KeepsParkingSpaceWhenAirborne { get; private set; } = true;
    /// <summary>
    /// this is how far behind us people aim when we are in attack mode
    /// </summary>
    public float SneakyOffsetWhenAttacking { get; private set; }
    public LocomotorSetType AttackLocomotorType { get; private set; }
    /// <summary>
    /// we start slowing down almost immediately
    /// </summary>
    public int AttackLocomotorPersistTime { get; private set; }
    /// <summary>
    /// but remain untargetable fer a bit longer
    /// </summary>
    public int AttackersMissPersistTime { get; private set; }
    public LocomotorSetType ReturnForAmmoLocomotorType { get; private set; }
    /// <summary>
    /// scooch it a little forward so the tail doesn't hit the doors
    /// </summary>
    public int ParkingOffset { get; private set; }
    /// <summary>
    /// if idle for this long, return to base, even if not out of ammo
    /// </summary>
    public LogicFrameSpan ReturnToBaseIdleTime { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public Percentage TakeoffDistForMaxLift { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new JetAIUpdate(gameObject, gameEngine, this);
    }
}
