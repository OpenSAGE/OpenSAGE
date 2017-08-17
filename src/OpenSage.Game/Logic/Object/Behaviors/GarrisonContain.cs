using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to use the GarrisonGun object definition for the weapons pointing from the object 
    /// when occupants are firing and these are drawn at bones named FIREPOINT. Also, it Allows use 
    /// of the GARRISONED Model ConditionState.
    /// </summary>
    public sealed class GarrisonContain : ObjectBehavior
    {
        internal static GarrisonContain Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GarrisonContain> FieldParseTable = new IniParseTable<GarrisonContain>
        {
            { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
            { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "MobileGarrison", (parser, x) => x.MobileGarrison = parser.ParseBoolean() },
            { "InitialRoster", (parser, x) => x.InitialRoster = GarrisonRoster.Parse(parser) },
        };

        public int ContainMax { get; private set; }
        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
        public float DamagePercentToUnits { get; private set; }
        public bool MobileGarrison { get; private set; }
        public GarrisonRoster InitialRoster { get; private set; }
    }

    public sealed class GarrisonRoster
    {
        internal static GarrisonRoster Parse(IniParser parser)
        {
            return new GarrisonRoster
            {
                ObjectName = parser.ParseAssetReference(),
                Quantity = parser.ParseInteger()
            };
        }

        public string ObjectName { get; private set; }
        public int Quantity { get; private set; }
    }
}
