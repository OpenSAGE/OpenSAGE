using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of the <see cref="ObjectDefinition.EnergyBonus"/> setting on this object to 
    /// provide extra power to the faction.
    /// </summary>
    public sealed class PowerPlantUpgrade : ObjectBehavior
    {
        internal static PowerPlantUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PowerPlantUpgrade> FieldParseTable = new IniParseTable<PowerPlantUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
    }
}
