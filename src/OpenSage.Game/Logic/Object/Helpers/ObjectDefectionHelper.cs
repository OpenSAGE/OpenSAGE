using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectDefectionHelper : ObjectHelperModule
    {
        // TODO

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown = reader.ReadBytes(13);
        }
    }
}
