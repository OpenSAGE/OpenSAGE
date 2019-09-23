using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to have particle system effects dynamically attached to animated 
    /// sub objects or bones.
    /// </summary>
    public sealed class AnimatedParticleSysBoneClientUpdateModuleData : ClientUpdateModuleData
    {
        internal static AnimatedParticleSysBoneClientUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AnimatedParticleSysBoneClientUpdateModuleData> FieldParseTable = new IniParseTable<AnimatedParticleSysBoneClientUpdateModuleData>();
    }
}
