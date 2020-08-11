using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object.Helpers
{
    internal abstract class ObjectHelperModule : UpdateModule
    {
        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }
}
