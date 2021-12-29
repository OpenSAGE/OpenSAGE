using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AnimatedParticleSysBoneClientUpdate : ClientUpdateModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

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
