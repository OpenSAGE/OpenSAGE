using System.IO;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class AnimatedParticleSysBoneClientUpdate : ClientUpdateModule
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

    /// <summary>
    /// Allows the object to have particle system effects dynamically attached to animated 
    /// sub objects or bones.
    /// </summary>
    public sealed class AnimatedParticleSysBoneClientUpdateModuleData : ClientUpdateModuleData
    {
        internal static AnimatedParticleSysBoneClientUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AnimatedParticleSysBoneClientUpdateModuleData> FieldParseTable = new IniParseTable<AnimatedParticleSysBoneClientUpdateModuleData>();

        internal override ClientUpdateModule CreateModule(Drawable drawable, GameContext context)
        {
            return new AnimatedParticleSysBoneClientUpdate();
        }
    }
}
