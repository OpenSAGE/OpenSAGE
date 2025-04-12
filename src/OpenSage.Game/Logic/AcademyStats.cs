using OpenSage.Logic.Object;

namespace OpenSage.Logic;

// TODO(Port): Add stats collection methods as needed.
public sealed class AcademyStats(IGame game) : IPersistableObject
{
    private readonly IGame _game = game;

    private LogicFrame _nextUpdateFrame;
    private bool _firstUpdate;
    private bool _unknownSide;

    // Tier 1 (Basic advice)

    // Did player run out of money before building a supply center?
    private bool _spentCashBeforeBuildingSupplyCenter;
    private uint _supplyCentersBuilt;
    private uint _supplyCenterCost;

    // Did player build radar (if applicable)?
    private bool _researchedRadar;

    // Did player build any dozers/workers?
    private uint _peonsBuilt;

    // Did player ever capture a structure?
    private uint _structuresCaptured;

    // Did player spend any generals points?
    private uint _generalsPointsSpent;

    // Did player ever use a generals power or superweapon?
    private uint _specialPowersUsed;

    // Did player garrison any structures?
    private uint _structuresGarrisoned;

    // How idle was the player in building military units?
    private uint _idleBuildingUnitsMaxFrames;
    private uint _lastUnitBuiltFrame;

    // Did player drag select units?
    private uint _dragSelectUnits;

    // Did player upgrade anything?
    private uint _upgradesPurchased;

    // Was player out of power for more than 10 minutes?
    private uint _powerOutMaxFrames;
    private uint _oldestPowerOutFrame;
    private bool _hadPowerLastCheck;

    // Extra gathers built?
    private uint _gatherersBuilt;

    // Heros built?
    private uint _heroesBuilt;

    // Tier 2 (Intermediate advice)

    // Selected a strategy center battle plan?
    private bool _hadAStrategyCenter;
    private bool _choseAStrategyForCenter;

    // Placed units inside tunnel network?
    private uint _unitsEnteredTunnelNetwork;
    private bool _hadATunnelNetwork;

    // Player used control groups?
    private uint _controlGroupsUsed;

    // Built secondary income unit (hacker, dropzone, blackmarket)?
    private uint _secondaryIncomeUnitsBuilt;

    // Cleared out garrisoned buildings?
    private uint _clearedGarrisonedBuildings;

    // Did the Player pick up salvage (as GLA)?
    private uint _salvageCollected;

    // Did the player ever use the "Guard" ability?
    private uint _guardAbilityUsedCount;

    // Tier 3 (Advanced advice)

    // Player did not use the new "double click location attack move/guard"
    private uint _doubleClickAttackMoveOrdersGiven;

    // Built barracks within 5 minutes?
    private bool _builtBarracksWithinFiveMinutes;

    // Built war factory within 10 minutes?
    private bool _builtWarFactoryWithinTenMinutes;

    // Built tech structure within 15 minutes?
    private bool _builtTechStructureWithinFifteenMinutes;

    // No income for 2 minutes?
    private LogicFrame _lastIncomeFrame;
    private LogicFrameSpan _maxFramesBetweenIncome;

    // Did the Player ever use Dozers/Workers to clear out traps/mines/booby traps?
    private uint _mines;
    private uint _minesCleared;

    // Captured any sniped vehicles?
    private uint _vehiclesRecovered;
    private uint _vehiclesSniped;

    // Did the player ever build a "disguisable" unit and never used the disguise ability?
    private uint _disguisableVehiclesBuilt;
    private uint _vehiclesDisguised;

    // Did the player ever create a "Firestorm" with his MiGs or Inferno Cannons?
    private uint _firestormsCreated;

    public void RecordIncome()
    {
        var now = _game.GameLogic.CurrentFrame;
        // Port note: ZH has a bug here, which I've decided to fix.
        // In ZH the subtraction is done in the wrong order, which is made worse by integer underflow.
        var delta = new LogicFrameSpan(now.Value - _lastIncomeFrame.Value);
        _maxFramesBetweenIncome = LogicFrameSpan.Max(_maxFramesBetweenIncome, delta);
        _lastIncomeFrame = now;
    }

    public void RecordClearedGarrisonedBuilding()
    {
        _clearedGarrisonedBuildings++;
    }
    public void RecordVehicleSniped()
    {
        _vehiclesSniped++;
    }

    // TOOD: After porting this I realised that this doesn't seem to be used anywhere in ZH?
    // Which means your stats will be reset every time you save & load a game.
    // We could add this to the savefile, but we'd have to increment the version of the Player xfer format.
    public void Persist(StatePersister persister)
    {
        persister.PersistVersion(1);

        persister.PersistLogicFrame(ref _nextUpdateFrame);
        persister.PersistBoolean(ref _firstUpdate);
        persister.PersistBoolean(ref _unknownSide);

        // Tier 1

        persister.PersistBoolean(ref _spentCashBeforeBuildingSupplyCenter);
        persister.PersistUInt32(ref _supplyCentersBuilt);
        persister.PersistUInt32(ref _supplyCenterCost);

        persister.PersistBoolean(ref _researchedRadar);

        persister.PersistUInt32(ref _peonsBuilt);

        persister.PersistUInt32(ref _structuresCaptured);

        persister.PersistUInt32(ref _generalsPointsSpent);

        persister.PersistUInt32(ref _specialPowersUsed);

        persister.PersistUInt32(ref _structuresGarrisoned);

        persister.PersistUInt32(ref _idleBuildingUnitsMaxFrames);
        persister.PersistUInt32(ref _lastUnitBuiltFrame);

        persister.PersistUInt32(ref _dragSelectUnits);

        persister.PersistUInt32(ref _upgradesPurchased);

        persister.PersistUInt32(ref _powerOutMaxFrames);
        persister.PersistUInt32(ref _oldestPowerOutFrame);
        persister.PersistBoolean(ref _hadPowerLastCheck);

        persister.PersistUInt32(ref _gatherersBuilt);

        persister.PersistUInt32(ref _heroesBuilt);

        // Tier 2

        persister.PersistBoolean(ref _hadAStrategyCenter);
        persister.PersistBoolean(ref _choseAStrategyForCenter);

        persister.PersistUInt32(ref _unitsEnteredTunnelNetwork);
        persister.PersistBoolean(ref _hadATunnelNetwork);

        persister.PersistUInt32(ref _controlGroupsUsed);

        persister.PersistUInt32(ref _secondaryIncomeUnitsBuilt);

        persister.PersistUInt32(ref _clearedGarrisonedBuildings);

        persister.PersistUInt32(ref _salvageCollected);

        persister.PersistUInt32(ref _guardAbilityUsedCount);

        // Tier 3

        persister.PersistUInt32(ref _doubleClickAttackMoveOrdersGiven);

        persister.PersistBoolean(ref _builtBarracksWithinFiveMinutes);

        persister.PersistBoolean(ref _builtWarFactoryWithinTenMinutes);

        persister.PersistBoolean(ref _builtTechStructureWithinFifteenMinutes);

        persister.PersistLogicFrame(ref _lastIncomeFrame);
        persister.PersistLogicFrameSpan(ref _maxFramesBetweenIncome);

        persister.PersistUInt32(ref _mines);
        persister.PersistUInt32(ref _minesCleared);

        persister.PersistUInt32(ref _vehiclesRecovered);
        persister.PersistUInt32(ref _vehiclesSniped);

        persister.PersistUInt32(ref _disguisableVehiclesBuilt);
        persister.PersistUInt32(ref _vehiclesDisguised);

        persister.PersistUInt32(ref _firestormsCreated);
    }
}
