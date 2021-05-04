using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule
    {
        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            // Maybe some kind of frame timer? But sometimes it's -2.
            var unknownInt1 = reader.ReadInt32();
        }
    }

    public abstract class UpdateModuleData : ContainModuleData
    {
        public override ModuleKind ModuleKind => ModuleKind.Update;
    }
}
