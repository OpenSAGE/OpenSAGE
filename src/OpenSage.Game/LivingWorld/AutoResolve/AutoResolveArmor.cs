using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.LivingWorld.AutoResolve
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveArmor
    {
        internal static AutoResolveArmor Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AutoResolveArmor> FieldParseTable = new IniParseTable<AutoResolveArmor>
        {
            { "Armor", (parser, x) => x.Values.Add(AutoResolveArmorValue.Parse(parser)) },
        };

        public string Name { get; private set; }

        public List<AutoResolveArmorValue> Values { get; } = new List<AutoResolveArmorValue>();
    }

    public sealed class AutoResolveArmorValue
    {
        internal static AutoResolveArmorValue Parse(IniParser parser)
        {
            return new AutoResolveArmorValue
            {
                DamageType = parser.ParseEnum<AutoResolveDamageType>(),
                Percent = parser.ParsePercentage()
            };
        }

        public AutoResolveDamageType DamageType { get; private set; }
        public Percentage Percent { get; private set; }
    }

    public enum AutoResolveDamageType
    {
        [IniEnum("DEFAULT")]
        Default = 0,

        [IniEnum("AutoResolveUnit_Archer")]
        Archer,

        [IniEnum("AutoResolveUnit_Soldier")]
        Soldier,

        [IniEnum("AutoResolveUnit_Pikemen")]
        Pikemen,

        [IniEnum("AutoResolveUnit_Cavalry")]
        Cavalry,

        [IniEnum("AutoResolveUnit_Hero")]
        Hero,

        [IniEnum("AutoResolveUnit_Monster")]
        Monster,

        [IniEnum("AutoResolveUnit_Fortress")]
        Fortress,

        [IniEnum("AutoResolveUnit_Siege"), AddedIn(SageGame.Bfme2Rotwk)]
        Siege,

        [IniEnum("AutoResolveUnit_Support"), AddedIn(SageGame.Bfme2Rotwk)]
        Support,
    }
}
