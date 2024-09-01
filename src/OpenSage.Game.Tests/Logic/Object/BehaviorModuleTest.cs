using OpenSage.Logic.Object;

namespace OpenSage.Tests.Logic.Object;

public abstract class BehaviorModuleTest<TModule, TData> : ModuleTest where TModule : BehaviorModule where TData : BehaviorModuleData, new()
{
    protected override byte[] ModuleData() => [V1, V1, .. base.ModuleData()];

    protected TModule SampleModule(IGame game = null, TData moduleData = null, GameObject gameObject = null) => (TModule)(moduleData ?? new TData()).CreateModule(gameObject, game?.Context);
}
