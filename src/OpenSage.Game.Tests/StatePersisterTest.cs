using System.IO;

namespace OpenSage.Tests;

public abstract class StatePersisterTest : MockedGameTest
{
    protected const byte V1 = 0x01;
    protected const byte V2 = 0x02;
    protected const byte V3 = 0x03;

    protected virtual MemoryStream SaveData(byte[] data, byte version = V1)
    {
        return new MemoryStream([version, .. data]);
    }

    protected virtual MemoryStream SaveDataNoVersion(byte[] data)
    {
        return new MemoryStream(data);
    }
}
