using Veldrid;

namespace OpenSage.Rendering;

public sealed class ShaderSetStore : DisposableBase
{
    private byte _nextId;

    public readonly GraphicsDevice GraphicsDevice;
    public readonly OutputDescription OutputDescription;

    public ShaderSetStore(GraphicsDevice graphicsDevice, OutputDescription outputDescription)
    {
        GraphicsDevice = graphicsDevice;
        OutputDescription = outputDescription;
    }

    internal byte GetNextId()
    {
        return checked(_nextId++);
    }
}
