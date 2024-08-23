using OpenSage.Logic.Object;

namespace OpenSage.Tests.Logic.Object.Update;

public abstract class UpdateModuleTest<TModule, TData> : BehaviorModuleTest<TModule, TData> where TModule : UpdateModule where TData : UpdateModuleData, new()
{
    private readonly byte[] _nextUpdateFrame = [0x00, 0x00, 0x00, 0x00];

    protected override byte[] ModuleData() => [V1, .. base.ModuleData(), .. _nextUpdateFrame];
}
