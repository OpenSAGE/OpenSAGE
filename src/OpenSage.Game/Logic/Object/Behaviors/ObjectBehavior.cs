using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ObjectBehavior : ObjectModule
    {
        internal static ObjectBehavior ParseBehavior(IniParser parser) => ParseModule(parser, BehaviorParseTable);

        private static readonly Dictionary<string, Func<IniParser, ObjectBehavior>> BehaviorParseTable = new Dictionary<string, Func<IniParser, ObjectBehavior>>
        {
            { "AIUpdateInterface", AIUpdateInterfaceBehavior.Parse },
            { "AutoDepositUpdate", AutoDepositUpdateBehavior.Parse },
            { "AutoHealBehavior", AutoHealBehavior.Parse },
            { "BaikonurLaunchPower", BaikonurLaunchPowerBehavior.Parse },
            { "BoneFXDamage", BoneFXDamageBehavior.Parse },
            { "BoneFXUpdate", BoneFXUpdateBehavior.Parse },
            { "BridgeBehavior", BridgeBehavior.Parse },
            { "BridgeTowerBehavior", BridgeTowerBehavior.Parse },
            { "CostModifierUpgrade", CostModifierUpgradeBehavior.Parse },
            { "CreateObjectDie", CreateObjectDieBehavior.Parse },
            { "DamDie", DamDieBehavior.Parse },
            { "DeletionUpdate", DeletionUpdateBehavior.Parse },
            { "DestroyDie", DestroyDieBehavior.Parse },
            { "FireWeaponWhenDeadBehavior", FireWeaponWhenDeadBehavior.Parse },
            { "FireWeaponWhenDamagedBehavior", FireWeaponWhenDamagedBehavior.Parse },
            { "FlammableUpdate", FlammableUpdateBehavior.Parse },
            { "FXListDie", FXListDieBehavior.Parse },
            { "GarrisonContain", GarrisonContainBehavior.Parse },
            { "GrantUpgradeCreate", GrantUpgradeCreateBehavior.Parse },
            { "KeepObjectDie", KeepObjectDieBehavior.Parse },
            { "LifetimeUpdate", LifetimeUpdateBehavior.Parse },
            { "MoneyCrateCollide", MoneyCrateCollideBehavior.Parse },
            { "PhysicsBehavior", PhysicsBehavior.Parse },
            { "SalvageCrateCollide", SalvageCrateCollideBehavior.Parse },
            { "SlowDeathBehavior", SlowDeathBehavior.Parse },
            { "SquishCollide", SquishCollideBehavior.Parse },
            { "StructureCollapseUpdate", StructureCollapseUpdateBehavior.Parse },
            { "StructureToppleUpdate", StructureToppleUpdateBehavior.Parse },
            { "SupplyWarehouseCreate", SupplyWarehouseCreateBehavior.Parse },
            { "SupplyWarehouseCripplingBehavior", SupplyWarehouseCripplingBehavior.Parse },
            { "SupplyWarehouseDockUpdate", SupplyWarehouseDockUpdateBehavior.Parse },
            { "TechBuildingBehavior", TechBuildingBehavior.Parse },
            { "ToppleUpdate", ToppleUpdateBehavior.Parse },
            { "TransitionDamageFX", TransitionDamageFXBehavior.Parse },
            { "UnitCrateCollide", UnitCrateCollideBehavior.Parse },
            { "VeterancyCrateCollide", VeterancyCrateCollideBehavior.Parse },
        };
    }
}
