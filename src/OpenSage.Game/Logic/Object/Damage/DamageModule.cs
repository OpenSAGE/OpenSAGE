using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public abstract class DamageModule : BehaviorModule
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

    public abstract class DamageModuleData : ContainModuleData
    {
    }
}
