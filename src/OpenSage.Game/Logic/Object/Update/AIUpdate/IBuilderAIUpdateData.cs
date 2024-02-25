using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

/// <summary>
/// Should be implemented by any module data class which inherits <see cref="AIUpdateModuleData"/> and is used by a <see cref="ObjectKinds.Dozer"/> unit.
/// </summary>
/// <remarks>
/// This interface is implemented by <see cref="DozerAIUpdateModuleData"/> and <see cref="WorkerAIUpdateModuleData"/>, since the GLA
/// worker is also a supply unit.
/// </remarks>
public interface IBuilderAIUpdateData
{
    public Percentage RepairHealthPercentPerSecond { get; }
    public LogicFrameSpan BoredTime { get; }
    public int BoredRange { get; }
}
