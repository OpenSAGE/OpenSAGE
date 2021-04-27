using System.IO;
using OpenSage.FileFormats;

namespace OpenSage
{
    public abstract class ModuleBase : DisposableBase
    {
        internal virtual void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }
        }
    }
}
