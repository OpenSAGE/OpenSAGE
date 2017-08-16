using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to have particle system effects dynamically attached to animated 
    /// sub objects or bones.
    /// </summary>
    public sealed class AnimatedParticleSysBoneClientUpdate : ClientUpdate
    {
        internal static AnimatedParticleSysBoneClientUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AnimatedParticleSysBoneClientUpdate> FieldParseTable = new IniParseTable<AnimatedParticleSysBoneClientUpdate>();
    }
}
