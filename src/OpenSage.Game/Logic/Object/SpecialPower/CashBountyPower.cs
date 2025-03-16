using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class CashBountyPower : SpecialPowerModule
{
    internal CashBountyPower(GameObject gameObject, GameEngine gameEngine, CashBountyPowerModuleData moduleData) : base(gameObject, gameEngine, moduleData)
    {
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Load(reader);
    }
}

public sealed class CashBountyPowerModuleData : SpecialPowerModuleData
{
    internal static new CashBountyPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<CashBountyPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
        .Concat(new IniParseTable<CashBountyPowerModuleData>
        {
            { "Bounty", (parser, x) => x.Bounty = parser.ParsePercentage() },
        });

    public Percentage Bounty { get; private set; }

    internal override CashBountyPower CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new CashBountyPower(gameObject, gameEngine, this);
    }
}
