using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Keeps this object attached properly to the intended target if the targetted object moves 
    /// and allows the use of UnitBombPing and StickyBombCreated within the UnitSpecificSounds section 
    /// of the object.
    /// </summary>
    public sealed class StickyBombUpdate : ObjectBehavior
    {
        internal static StickyBombUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StickyBombUpdate> FieldParseTable = new IniParseTable<StickyBombUpdate>();
    }
}
