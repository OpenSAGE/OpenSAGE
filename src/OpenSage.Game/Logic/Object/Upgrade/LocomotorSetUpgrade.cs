using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of SET_NORMAL_UPGRADED locomotor on this object and allows the use of 
    /// VoiceMoveUpgrade within the UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class LocomotorSetUpgrade : ObjectBehavior
    {
        internal static LocomotorSetUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LocomotorSetUpgrade> FieldParseTable = new IniParseTable<LocomotorSetUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
    }
}
