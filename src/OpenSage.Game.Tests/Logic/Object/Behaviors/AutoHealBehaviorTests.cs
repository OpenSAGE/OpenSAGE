using OpenSage.Logic.Object;
using OpenSage.Tests.Logic.Object.Update;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class AutoHealBehaviorTests : UpdateModuleTest<AutoHealBehavior, AutoHealBehaviorModuleData>
{
    /// <summary>
    /// A fresh ambulance has the upgrade set to active as <c>StartsActive</c> is set to true in the module data.
    /// </summary>
    /// <remarks>
    /// This also seemed to be the save file data for a TechHospital and RepairVehiclesInArea_InvisibleMarker_Level1.
    /// </remarks>
    private static readonly byte[] AmericaAmbulanceFreshSpawn = [0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void Generals_Ambulance_FreshSpawn()
    {
        var stream = SaveData(AmericaAmbulanceFreshSpawn);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);
    }

    /// <summary>
    /// An ambulance which is healing will have data set to store the next frame when healing should be applied.
    /// </summary>
    private static readonly byte[] AmericaAmbulanceHealing = [0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x8a, 0x17, 0x00, 0x00, 0x00];

    [Fact]
    public void Generals_Ambulance_Healing()
    {
        var stream = SaveData(AmericaAmbulanceHealing);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule();
        behavior.Load(reader);
    }
}
