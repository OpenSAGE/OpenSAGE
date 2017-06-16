using System.IO;

namespace Pfim
{
    internal interface IDecodeDds
    {
        DdsLoadInfo ImageInfo(DdsHeader header);
        byte[] Decode(Stream str, DdsHeader header);
    }
}
