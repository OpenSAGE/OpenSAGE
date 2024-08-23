using System.IO;

namespace OpenSage.Tests.Logic.Object;

public abstract class ModuleTest : MockedGameTest
{
    protected const byte V1 = 0x01;
    protected virtual byte[] ModuleData() => [V1];

    protected MemoryStream SaveData(byte[] data, byte version = V1)
    {
        return new MemoryStream([version, .. ModuleData(), ..data]);
    }
}
