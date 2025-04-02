using System;

namespace OpenSage.Logic.Object;

public interface IContainModule
{
    bool IsGarrisonable { get; }
    bool IsImmuneToClearBuildingAttacks { get; }
    bool IsRiderChangeContain { get; }

    uint ContainCount { get; }
    float ContainedItemsMass { get; }

    ReadOnlySpan<GameObject> ContainedItems { get; }

    void OrderAllPassengersToIdle(CommandSourceType commandType);
    void OrderAllPassengersToHackInternet(CommandSourceType commandType);
}
