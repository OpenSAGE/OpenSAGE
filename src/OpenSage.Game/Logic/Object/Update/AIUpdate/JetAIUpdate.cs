using System;
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

    internal JetAIUpdate(GameObject gameObject, GameContext context, JetAIUpdateModuleData moduleData)
        : base(gameObject, context, moduleData)
    {
        ModuleData = moduleData;
        CurrentJetAIState = JetAIState.Parked;
    }

    private protected override JetAIUpdateStateMachine CreateStateMachine() => new(GameObject, Context, this);

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

    internal override void Update(BehaviorUpdateContext context)
    {
        base.Update(context);

        if (!ModuleData.KeepsParkingSpaceWhenAirborne)
        {
            return; // helicopters are way more simple (at least for now)
        }

        if (Base == null)
        {
            // fix for mapObject aircrafts etc (e.g. ZH shellmap)
            // TODO: handle scenario of a destroyed airfield
            return;
        }

        var parkingPlaceBehavior = Base.FindBehavior<ParkingPlaceBehaviour>();

        var isMoving = GameObject.ModelConditionFlags.Get(ModelConditionFlag.Moving);

        var trans = GameObject.Translation;

        var terrainHeight = context.GameContext.Terrain.HeightMap.GetHeight(trans.X, trans.Y);

        switch (CurrentJetAIState)
        {
            case JetAIState.JustCreated:
                var parkingTransform = parkingPlaceBehavior.GetParkingTransform(GameObject.ID);
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
                var createTransform = Base.ToWorldspace(parkingPlaceBehavior.GetUnitCreateTransform(GameObject.ID));
                SetTargetDirection(createTransform.LookDirection);
                CurrentJetAIState = JetAIState.Rotating;
                break;

            case JetAIState.Rotating:
                if (isMoving)
                {
                    break;
                }

                //base.SetTargetPoint(GameObject.Transform.Translation + GameObject.Transform.LookDirection * _moduleData.ParkingOffset);
                parkingPlaceBehavior.ReportParkedIdle(GameObject.ID, context.LogicFrame);
                CurrentJetAIState = JetAIState.Parked;
                break;

            case JetAIState.Parked:
                if (_unparkingRequested)
                {
                    if (!parkingPlaceBehavior.ReportReadyToTaxi(GameObject.ID, out var runway))
                    {
                        break;
                    }
                    _pathToStart = parkingPlaceBehavior.GetPathToRunway(GameObject.ID, runway);
                    CurrentJetAIState = JetAIState.MovingTowardsStart;
                    _unparkingRequested = false;
                }
                break;

            case JetAIState.MovingTowardsStart:
                if (isMoving || ProcessWaypointPath(context, parkingPlaceBehavior, _pathToStart))
                {
                    break;
                }

                parkingPlaceBehavior.ReportEngineRunUp(GameObject.ID);
                CurrentJetAIState = JetAIState.PreparingStart;
                _waitUntil = context.LogicFrame + ModuleData.TakeoffPause;
                break;

            case JetAIState.PreparingStart:
                if (context.LogicFrame < _waitUntil)
                {
                    break;
                }

                SetLocomotor(LocomotorSetType.Normal);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetExhaust, true);
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, true);
                _afterburnerEnabled = true;
                var endPointPosition =
                    Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject.ID));
                base.SetTargetPoint(endPointPosition);
                AddTargetPoint(_currentTargetPoint);
                CurrentJetAIState = JetAIState.Starting;
                CurrentLocomotor.LiftFactor = 0;
                break;

            case JetAIState.Starting:
                var speedPercentage = GameObject.Speed / CurrentLocomotor.GetSpeed();
                CurrentLocomotor.LiftFactor = speedPercentage;

                if (speedPercentage < ModuleData.TakeoffSpeedForMaxLift)
                {
                    break;
                }

                // todo: this actually shouldn't happen until we're above a certain altitude
                parkingPlaceBehavior.ReportDeparted(GameObject.ID);
                CurrentJetAIState = JetAIState.MovingTowardsTarget;
                break;

            case JetAIState.MovingTowardsTarget:
                if (isMoving)
                {
                    if (_afterburnerEnabled && trans.Z - terrainHeight >= CurrentLocomotor.GetLocomotorValue(_ => _.PreferredHeight))
                    {
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.JetAfterburner, false);
                        _afterburnerEnabled = false;
                    }
                    break;
                }
                CurrentJetAIState = JetAIState.ReachedTargetPoint;
                _waitUntil = context.LogicFrame + ModuleData.ReturnToBaseIdleTime;
                break;

            case JetAIState.ReachedTargetPoint:
                if (context.LogicFrame < _waitUntil)
                {
                    break;
                }

                parkingPlaceBehavior.ReportInbound(GameObject.ID);
                var endPosition = Base.ToWorldspace(parkingPlaceBehavior.GetRunwayEndPoint(GameObject.ID));

                base.SetTargetPoint(endPosition);
                CurrentJetAIState = JetAIState.Landing;
                break;
            case JetAIState.Landing:
                parkingPlaceBehavior.ReportLanding(GameObject.ID);
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
                var (landedRunway, parkingSlot) = parkingPlaceBehavior.ReportLanded(GameObject.ID);
                _pathToParking = parkingPlaceBehavior.GetPathToHangar(landedRunway, parkingSlot);
                break;

            case JetAIState.MovingBackToHangar:
                if (isMoving || ProcessWaypointPath(context, parkingPlaceBehavior, _pathToParking))
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
            Base.ProductionUpdate?.CloseDoor(parkingPlaceBehavior.ClearObjectFromSlot(GameObject.ID));
        }
    }

    private bool ProcessWaypointPath(BehaviorUpdateContext context, ParkingPlaceBehaviour parkingPlaceBehavior, Queue<string> path)
    {
        if (_currentTaxiingTarget != null)
        {
            parkingPlaceBehavior.ClearRunway(GameObject.ID);
        }

        if (path.Count > 0)
        {
            var nextPoint = path.Peek();
            if (parkingPlaceBehavior.IsTaxiingPointBlocked(nextPoint))
            {
                _waitUntil = context.LogicFrame + ModuleData.TakeoffPause;
                return true;
            }
            if (context.LogicFrame < _waitUntil)
            {
                return true;
            }

            _currentTaxiingTarget = nextPoint;
            parkingPlaceBehavior.ReserveRunway(GameObject.ID);
            base.SetTargetPoint(Base.ToWorldspace(parkingPlaceBehavior.GetBoneTranslation(path.Dequeue())));
            return true;
        }

        return false;
    }
}

internal sealed class JetAIUpdateStateMachine : AIUpdateStateMachine
{
    public override JetAIUpdate AIUpdate { get; }

    public JetAIUpdateStateMachine(GameObject gameObject, GameContext context, JetAIUpdate aiUpdate)
        : base(gameObject, context, aiUpdate)
    {
        AIUpdate = aiUpdate;

        AddState(WaitForAirfieldWinchesterState.StateId, new WaitForAirfieldWinchesterState(this));
    }
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

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new JetAIUpdate(gameObject, context, this);
    }
}
