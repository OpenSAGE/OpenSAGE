using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class BehaviorModule : ModuleBase
    {
        internal virtual void Update(BehaviorUpdateContext context) { }

        internal virtual void OnDie(BehaviorUpdateContext context, DeathType deathType) { }

        internal virtual void OnDamageStateChanged(BehaviorUpdateContext context, BodyDamageType fromDamage, BodyDamageType toDamage) { }

        internal virtual void OnCollide(BehaviorUpdateContext context, GameObject collidingObject) { }

        internal virtual void DrawInspector() { }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            // The following version number is probably from extra base class in the inheritance hierarchy.
            // Since we don't have that at the moment, just read it here.
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    internal sealed class BehaviorUpdateContext
    {
        public readonly GameContext GameContext;
        public readonly GameObject GameObject;
        public TimeInterval Time { get; private set; }

        public BehaviorUpdateContext(
            GameContext gameContext,
            GameObject gameObject,
            in TimeInterval time)
        {
            GameContext = gameContext;
            GameObject = gameObject;
            Time = time;
        }

        public void UpdateTime(TimeInterval time)
        {
            Time = time;
        }
    }

    public abstract class BehaviorModuleData : ModuleData
    {
        internal static ModuleDataContainer ParseBehavior(IniParser parser, ModuleInheritanceMode inheritanceMode) => ParseModule(parser, BehaviorParseTable, inheritanceMode);

        private static readonly Dictionary<string, Func<IniParser, BehaviorModuleData>> BehaviorParseTable = new Dictionary<string, Func<IniParser, BehaviorModuleData>>
        {
            // Behavior
            { "AimWeaponBehavior", AimWeaponBehaviorModuleData.Parse },
            { "AnnounceBirthAndDeathBehavior", AnnounceBirthAndDeathBehaviorModuleData.Parse },
            { "AutoAbilityBehavior", AutoAbilityBehaviorModuleData.Parse },
            { "AutoHealBehavior", AutoHealBehaviorModuleData.Parse },
            { "BattleBusSlowDeathBehavior", BattleBusSlowDeathBehaviorModuleData.Parse },
            { "BridgeBehavior", BridgeBehaviorModuleData.Parse },
            { "BridgeScaffoldBehavior", BridgeScaffoldBehaviorModuleData.Parse },
            { "BridgeTowerBehavior", BridgeTowerBehaviorModuleData.Parse },
            { "BuildingBehavior", BuildingBehaviorModuleData.Parse },
            { "BunkerBusterBehavior", BunkerBusterBehaviorModuleData.Parse },
            { "CastleBehavior", CastleBehaviorModuleData.Parse },
            { "CastleMemberBehavior", CastleMemberBehaviorModuleData.Parse },
            { "ClearanceTestingSlowDeathBehavior", ClearanceTestingSlowDeathBehaviorModuleData.Parse },
            { "ClickReactionBehavior", ClickReactionBehaviorData.Parse },
            { "CountermeasuresBehavior", CountermeasuresBehaviorModuleData.Parse },
            { "DumbProjectileBehavior", BezierProjectileBehaviorData.Parse },
            { "DualWeaponBehavior", DualWeaponBehaviorModuleData.Parse },
            { "DynamicPortalBehaviour", DynamicPortalBehaviorModuleData.Parse },
            { "FakePathfindPortalBehaviour", FakePathfindPortalBehaviourModuleData.Parse },
            { "FireWeaponWhenDeadBehavior", FireWeaponWhenDeadBehaviorModuleData.Parse },
            { "FireWeaponWhenDamagedBehavior", FireWeaponWhenDamagedBehaviorModuleData.Parse },
            { "FlightDeckBehavior", FlightDeckBehaviorModuleData.Parse },
            { "GateOpenAndCloseBehavior", GateOpenAndCloseBehaviorModuleData.Parse },
            { "GateProxyBehavior", GateProxyBehaviorModuleData.Parse },
            { "GenerateMinefieldBehavior", GenerateMinefieldBehaviorModuleData.Parse },
            { "GettingBuiltBehavior", GettingBuiltBehaviorModuleData.Parse },
            { "GiantBirdSlowDeathBehavior", GiantBirdSlowDeathBehaviorModuleData.Parse },
            { "GrantStealthBehavior", GrantStealthBehaviorModuleData.Parse },
            { "HelicopterSlowDeathBehavior", HelicopterSlowDeathBehaviorModuleData.Parse },
            { "InstantDeathBehavior", InstantDeathBehaviorModuleData.Parse },
            { "JetSlowDeathBehavior", JetSlowDeathBehaviorModuleData.Parse },
            { "LeafletDropBehavior", LeafletDropBehaviorModuleData.Parse },
            { "MinefieldBehavior", MinefieldBehaviorModuleData.Parse },
            { "NeutronBlastBehavior", NeutronBlastBehaviorModuleData.Parse },
            { "NeutronMissileSlowDeathBehavior", NeutronMissileSlowDeathBehaviorModuleData.Parse },
            { "OathbreakersFadeAwayBehavior", OathbreakersFadeAwayBehaviorModuleData.Parse },
            { "OverchargeBehavior", OverchargeBehaviorModuleData.Parse },
            { "ParkingPlaceBehavior", ParkingPlaceBehaviorModuleData.Parse },
            { "PassiveAreaEffectBehavior", PassiveAreaEffectBehaviorModuleData.Parse },
            { "PhysicsBehavior", PhysicsBehaviorModuleData.Parse },
            { "PoisonedBehavior", PoisonedBehaviorModuleData.Parse },
            { "PropagandaTowerBehavior", PropagandaTowerBehaviorModuleData.Parse },
            { "RailroadBehavior", RailroadBehaviorModuleData.Parse },
            { "RebuildHoleBehavior", RebuildHoleBehaviorModuleData.Parse },
            { "ReplenishUnitsBehavior", ReplenishUnitsBehaviorModuleData.Parse },
            { "RunOffMapBehavior", RunOffMapBehaviorModuleData.Parse },
            { "ShareExperienceBehavior", ShareExperienceBehaviorModuleData.Parse },
            { "ShipSlowDeathBehavior", ShipSlowDeathBehaviorModuleData.Parse },
            { "SiegeDockingBehavior", SiegeDockingBehaviorModuleData.Parse },
            { "SlaveWatcherBehavior", SlaveWatcherBehaviorModuleData.Parse },
            { "SlowDeathBehavior", SlowDeathBehaviorModuleData.Parse },
            { "SpawnBehavior", SpawnBehaviorModuleData.Parse },
            { "SpawnUnitBehavior", SpawnUnitBehaviorModuleData.Parse },
            { "StancesBehavior", StancesBehaviorModuleData.Parse },
            { "SupplyWarehouseCripplingBehavior", SupplyWarehouseCripplingBehaviorModuleData.Parse },
            { "TechBuildingBehavior", TechBuildingBehaviorModuleData.Parse },
            { "BezierProjectileBehavior", BezierProjectileBehaviorData.Parse },
            { "HitReactionBehavior", HitReactionBehaviorData.Parse },
            { "TerrainResourceBehavior", TerrainResourceBehaviorModuleData.Parse },
            { "WallHubBehavior", WallHubBehaviorModuleData.Parse },

            // Collide
            { "CivilianSpawnCollide", CivilianSpawnCollideModuleData.Parse },
            { "ConvertToCarBombCrateCollide", ConvertToCarBombCrateCollideModuleData.Parse },
            { "ConvertToHijackedVehicleCrateCollide", ConvertToHijackedVehicleCrateCollideModuleData.Parse },
            { "AODCrushCollide", AODCrushCollideModuleData.Parse },
            { "FireWeaponCollide", FireWeaponCollideModuleData.Parse },
            { "HordeMemberCollide", HordeMemberCollideModuleData.Parse },
            { "MoneyCrateCollide", MoneyCrateCollideModuleData.Parse },
            { "SabotageCommandCenterCrateCollide", SabotageCommandCenterCrateCollideModuleData.Parse },
            { "SabotageFakeBuildingCrateCollide", SabotageFakeBuildingCrateCollideModuleData.Parse },
            { "SabotageInternetCenterCrateCollide", SabotageInternetCenterCrateCollideModuleData.Parse },
            { "SabotageMilitaryFactoryCrateCollide", SabotageMilitaryFactoryCrateCollideModuleData.Parse },
            { "SabotagePowerPlantCrateCollide", SabotagePowerPlantCrateCollideModuleData.Parse },
            { "SabotageSuperweaponCrateCollide", SabotageSuperweaponCrateCollideModuleData.Parse },
            { "SabotageSupplyCenterCrateCollide", SabotageSupplyCenterCrateCollideModuleData.Parse },
            { "SalvageCrateCollide", SalvageCrateCollideModuleData.Parse },
            { "SquishCollide", SquishCollideModuleData.Parse },
            { "UnitCrateCollide", UnitCrateCollideModuleData.Parse },
            { "VeterancyCrateCollide", VeterancyCrateCollideModuleData.Parse },

            // Contain
            { "AODHordeContain", AodHordeContainModuleData.Parse },
            { "CitadelSlaughterHordeContain", CitadelSlaughterHordeContainModuleData.Parse },
            { "GarrisonContain", GarrisonContainModuleData.Parse },
            { "HealContain", HealContainModuleData.Parse },
            { "HelixContain", HelixContainModuleData.Parse },
            { "HordeContain", HordeContainModuleData.Parse },
            { "HordeGarrisonContain", HordeGarrisonContainModuleData.Parse },
            { "HordeSiegeEngineContain", HordeSiegeEngineContainModuleData.Parse },
            { "HordeTransportContain", HordeTransportContainModuleData.Parse },
            { "HorseHordeContain", HorseHordeContainModuleData.Parse },
            { "InternetHackContain", InternetHackContainModuleData.Parse },
            { "OverlordContain", OverlordContainModuleData.Parse },
            { "ParachuteContain", ParachuteContainModuleData.Parse },
            { "ProductionQueueHordeContain", ProductionQueueHordeContainModuleData.Parse },
            { "RailedTransportContain", RailedTransportContainModuleData.Parse },
            { "RiderChangeContain", RiderChangeContainModuleData.Parse },
            { "SiegeEngineContain", SiegeEngineContainModuleData.Parse },
            { "SlaughterHordeContain", SlaughterHordeContainModuleData.Parse },
            { "TransportContain", TransportContainModuleData.Parse },
            { "TunnelContain", TunnelContainModuleData.Parse },

            // Create
            { "ExperienceLevelCreate", ExperienceLevelCreateModuleData.Parse },
            { "GrantUpgradeCreate", GrantUpgradeCreateModuleData.Parse },
            { "InheritUpgradeCreate", InheritUpgradeCreateModuleData.Parse },
            { "LockWeaponCreate", LockWeaponCreateModuleData.Parse },
            { "PreorderCreate", PreorderCreateModuleData.Parse },
            { "SpecialPowerCreate", SpecialPowerCreateModuleData.Parse },
            { "SupplyCenterCreate", SupplyCenterCreateModuleData.Parse },
            { "SupplyWarehouseCreate", SupplyWarehouseCreateModuleData.Parse },
            { "VeterancyGainCreate", VeterancyGainCreateModuleData.Parse },

            // Damage
            { "BoneFXDamage", BoneFXDamageModuleData.Parse },
            { "EvacuateDamage", EvacuateDamageModuleData.Parse },
            { "HordeTransportContainDamage", HordeTransportContainDamageModuleData.Parse },
            { "ReflectDamage", ReflectDamageModuleData.Parse },
            { "TransitionDamageFX", TransitionDamageFXModuleData.Parse },

            // Die
            { "CreateCrateDie", CreateCrateDieModuleData.Parse },
            { "CreateObjectDie", CreateObjectDieModuleData.Parse },
            { "CrushDie", CrushDieModuleData.Parse },
            { "DamageFilteredCreateObjectDie", DamageFilteredCreateObjectDieModuleData.Parse },
            { "DamDie", DamDieModuleData.Parse },
            { "DestroyDie", DestroyDieModuleData.Parse },
            { "EjectPilotDie", EjectPilotDieModuleData.Parse },
            { "FXListDie", FXListDieModuleData.Parse },
            { "HeroDie", HeroDieModuleData.Parse },
            { "KeepObjectDie", KeepObjectDieModuleData.Parse },
            { "RebuildHoleExposeDie", RebuildHoleExposeDieModuleData.Parse },
            { "RefundDie", RefundDieModuleData.Parse },
            { "SpecialPowerCompletionDie", SpecialPowerCompletionDieModuleData.Parse },
            { "UpgradeDie", UpgradeDieModuleData.Parse },

            //Module
            { "PillageModule", PillageModuleData.Parse },
            { "SpecialPowerModule", SpecialPowerModuleData.Parse },
            { "WeaponChangeSpecialPowerModule", WeaponChangeSpecialPowerModuleData.Parse },

            // SpecialPower
            { "ActivateModuleSpecialPower", ActivateModuleSpecialPowerModuleData.Parse },
            { "BaikonurLaunchPower", BaikonurLaunchPowerModuleData.Parse },
            { "CashBountyPower", CashBountyPowerModuleData.Parse },
            { "CashHackSpecialPower", CashHackSpecialPowerModuleData.Parse },
            { "CleanupAreaPower", CleanupAreaPowerModuleData.Parse },
            { "CloudBreakSpecialPower", CloudBreakSpecialPowerModuleData.Parse },
            { "CombineHordeSpecialPower", CombineHordeSpecialPowerModuleData.Parse },
            { "CurseSpecialPower", CurseSpecialPowerModuleData.Parse },
            { "DarknessSpecialPower", DarknessSpecialPowerModuleData.Parse },
            { "DefectorSpecialPower", DefectorSpecialPowerModuleData.Parse },
            { "DeflectSpecialPower", DeflectSpecialPowerModuleData.Parse },
            { "DevastateSpecialPower", DevastateSpecialPowerModuleData.Parse },
            { "DominateEnemySpecialPower", DominateEnemySpecialPowerModuleData.Parse },
            { "ElvenWoodSpecialPower", ElvenWoodSpecialPowerModuleData.Parse },
            { "FellBeastSwoopPower", FellBeastSwoopPowerModuleData.Parse },
            { "FireWeaponPower", FireWeaponPowerModuleData.Parse },
            { "FreezingRainSpecialPower", FreezingRainSpecialPowerModuleData.Parse },
            { "GrabPassengerSpecialPower", GrabPassengerSpecialPowerModuleData.Parse },
            { "HordeDispatchSpecialPower", HordeDispatchSpecialPowerModuleData.Parse },
            { "InvisibilitySpecialPower", InvisibilitySpecialPowerModuleData.Parse },
            { "LevelGrantSpecialPower", LevelGrantSpecialPowerModuleData.Parse },
            { "ManTheWallsSpecialPower", ManTheWallsSpecialPowerModuleData.Parse },
            { "OCLSpecialPower", OCLSpecialPowerModuleData.Parse },
            { "PlayerHealSpecialPower", PlayerHealSpecialPowerModuleData.Parse },
            { "PlayerUpgradeSpecialPower", PlayerUpgradeSpecialPowerModuleData.Parse },
            { "ProductionSpeedBonus", ProductionSpeedBonusModuleData.Parse },
            { "RepairSpecialPower", RepairSpecialPowerModuleData.Parse },
            { "ScavengerSpecialPower", ScavengerSpecialPowerModuleData.Parse },
            { "SiegeDeploySpecialPower", SiegeDeploySpecialPowerModuleData.Parse },
            { "SiegeDeployHordeSpecialPower", SiegeDeployHordeSpecialPowerModuleData.Parse },
            { "SpecialAbility", SpecialAbilityModuleData.Parse },
            { "SpecialPowerTimerRefreshSpecialPower", SpecialPowerTimerRefreshSpecialPowerModuleData.Parse },
            { "SplitHordeSpecialPower", SplitHordeSpecialPowerModuleData.Parse },
            { "SpyVisionSpecialPower", SpyVisionSpecialPowerModuleData.Parse },
            { "StopSpecialPower", StopSpecialPowerModuleData.Parse },
            { "StoreObjectsSpecialPower", StoreObjectsSpecialPowerModuleData.Parse },
            { "TaintSpecialPower", TaintSpecialPowerModuleData.Parse },
            { "TeleportToCasterSpecialPower", TeleportToCasterSpecialPowerModuleData.Parse },
            { "UnleashSpecialPower", UnleashSpecialPowerModuleData.Parse },
            { "UntamedAllegianceSpecialPower", UntamedAllegianceSpecialPowerModuleData.Parse },

            // Update
            { "AnimationSteeringUpdate", AnimationSteeringUpdateModuleData.Parse },
            { "ArrowStormUpdate", ArrowStormUpdateModuleData.Parse },
            { "AssistedTargetingUpdate", AssistedTargetingUpdateModuleData.Parse },
            { "AttachUpdate", AttachUpdateModuleData.Parse },
            { "AttributeModifierPoolUpdate", AttributeModifierPoolUpdateModuleData.Parse },
            { "AttributeModifierAuraUpdate", AttributeModifierAuraUpdateModuleData.Parse },
            { "AutoDepositUpdate", AutoDepositUpdateModuleData.Parse },
            { "AutoFindHealingUpdate", AutoFindHealingUpdateModuleData.Parse },
            { "AutoPickUpUpdate", AutoPickUpUpdateModuleData.Parse },
            { "BannerCarrierUpdate", BannerCarrierUpdateModuleData.Parse },
            { "BaseRegenerateUpdate", BaseRegenerateUpdateModuleData.Parse },
            { "BattlePlanUpdate", BattlePlanUpdateModuleData.Parse },
            { "BloodthirstyUpdate", BloodthirstyUpdateModuleData.Parse },
            { "BoneFXUpdate", BoneFXUpdateModuleData.Parse },
            { "BoredUpdate", BoredUpdateModuleData.Parse },
            { "CheckpointUpdate", CheckpointUpdateModuleData.Parse },
            { "CivilianSpawnUpdate", CivilianSpawnUpdateModuleData.Parse },
            { "CleanupHazardUpdate", CleanupHazardUpdateModuleData.Parse },
            { "CommandButtonHuntUpdate", CommandButtonHuntUpdateModuleData.Parse },
            { "CritterEmitterUpdate", CritterEmitterUpdateModuleData.Parse },
            { "DamageFieldUpdate", DamageFieldUpdateModuleData.Parse },
            { "DefaultProductionExitUpdate", DefaultProductionExitUpdateModuleData.Parse },
            { "DelayedLuaEventUpdate", DelayedLuaEventUpdateModuleData.Parse },
            { "DeletionUpdate", DeletionUpdateModuleData.Parse },
            { "DemoTrapUpdate", DemoTrapUpdateModuleData.Parse },
            { "DestroyEnvironmentUpdate", DestroyEnvironmentUpdateModuleData.Parse },
            { "DetachableRiderUpdate", DetachableRiderUpdateModuleData.Parse },
            { "DynamicShroudClearingRangeUpdate", DynamicShroudClearingRangeUpdateModuleData.Parse },
            { "EmotionTrackerUpdate", EmotionTrackerUpdateModuleData.Parse },
            { "EMPUpdate", EmpUpdateModuleData.Parse },
            { "EnemyNearUpdate", EnemyNearUpdateModuleData.Parse },
            { "EntEnragedUpdate", EntEnragedUpdateModuleData.Parse },
            { "FadeAndDieOrnamentUpdate", FadeAndDieOrnamentUpdateModuleData.Parse },
            { "FireOCLAfterWeaponCooldownUpdate", FireOCLAfterWeaponCooldownUpdateModuleData.Parse },
            { "FireSpreadUpdate", FireSpreadUpdateModuleData.Parse },
            { "FirestormDynamicGeometryInfoUpdate", FirestormDynamicGeometryInfoUpdateModuleData.Parse },
            { "FireWeaponUpdate", FireWeaponUpdateModuleData.Parse },
            { "FlammableUpdate", FlammableUpdateModuleData.Parse },
            { "FloatUpdate", FloatUpdateModuleData.Parse },
            { "FloodUpdate", FloodUpdateModuleData.Parse },
            { "GiveUpgradeUpdate", GiveUpgradeUpdateModuleData.Parse },
            { "HeightDieUpdate", HeightDieUpdateModuleData.Parse },
            { "HijackerUpdate", HijackerUpdateModuleData.Parse },
            { "HordeNotifyTargetsOfImminentProbableCrushingUpdate", HordeNotifyTargetsOfImminentProbableCrushingUpdateModuleData.Parse },
            { "HordeUpdate", HordeUpdateModuleData.Parse },
            { "InvisibilityUpdate", InvisibilityUpdateModuleData.Parse },
            { "LargeGroupAudioUpdate", LargeGroupAudioUpdateModuleData.Parse },
            { "LargeGroupBonusUpdate", LargeGroupBonusUpdateModuleData.Parse },
            { "LifetimeUpdate", LifetimeUpdateModuleData.Parse },
            { "MissileLauncherBuildingUpdate", MissileLauncherBuildingUpdateModuleData.Parse },
            { "MobMemberSlavedUpdate", MobMemberSlavedUpdateModuleData.Parse },
            { "MonitorConditionUpdate", MonitorConditionUpdateModuleData.Parse },
            { "NeutronMissileUpdate", NeutronMissileUpdateModuleData.Parse },
            { "NotifyTargetsOfImminentProbableCrushingUpdate", NotifyTargetsOfImminentProbableCrushingUpdateModuleData.Parse },
            { "OCLUpdate", OCLUpdateModuleData.Parse },
            { "OilSpillUpdate", OilSpillUpdateModuleData.Parse },
            { "OneRingPenaltyUpdate", OneRingPenaltyUpdateModuleData.Parse },
            { "ParticleUplinkCannonUpdate", ParticleUplinkCannonUpdateModuleData.Parse },
            { "PartTheHeavensUpdate", PartTheHeavensUpdateModuleData.Parse },
            { "PickupStuffUpdate", PickupStuffUpdateModuleData.Parse },
            { "PilotFindVehicleUpdate", PilotFindVehicleUpdateModuleData.Parse },
            { "PointDefenseLaserUpdate", PointDefenseLaserUpdateModuleData.Parse },
            { "PowerPlantUpdate", PowerPlantUpdateModuleData.Parse },
            { "ProductionUpdate", ProductionUpdateModuleData.Parse },
            { "ProjectileStreamUpdate", ProjectileStreamUpdateModuleData.Parse },
            { "QueueProductionExitUpdate", QueueProductionExitUpdateModuleData.Parse },
            { "RadarUpdate", RadarUpdateModuleData.Parse },
            { "RadiateFearUpdate", RadiateFearUpdateModuleData.Parse },
            { "RadiusDecalUpdate", RadiusDecalUpdateModuleData.Parse },
            { "RailedTransportDockUpdate", RailedTransportDockUpdateModuleData.Parse },
            { "RainOfFireUpdate", RainOfFireUpdateModuleData.Parse },
            { "RepairDockUpdate", RepairDockUpdateModuleData.Parse },
            { "ReplaceObjectUpdate", ReplaceObjectUpdateModuleData.Parse },
            { "RespawnUpdate", RespawnUpdateModuleData.Parse },
            { "RousingSpeechUpdate", RousingSpeechUpdateModuleData.Parse },
            { "RubbleRiseUpdate", RubbleRiseUpdateModuleData.Parse },
            { "SlavedUpdate", SlavedUpdateModuleData.Parse },
            { "SmartBombTargetHomingUpdate", SmartBombTargetHomingUpdateModuleData.Parse },
            { "SpawnPointProductionExitUpdate", SpawnPointProductionExitUpdateModuleData.Parse },
            { "SpecialDisguiseUpdate", SpecialDisguiseUpdateModuleData.Parse },
            { "SpecialEnemySenseUpdate", SpecialEnemySenseUpdateModuleData.Parse },
            { "SpectreGunshipUpdate", SpectreGunshipUpdateModuleData.Parse },
            { "SpectreGunshipDeploymentUpdate", SpectreGunshipDeploymentUpdateModuleData.Parse },
            { "SpyVisionUpdate", SpyVisionUpdateModuleData.Parse },
            { "StealthDetectorUpdate", StealthDetectorUpdateModuleData.Parse },
            { "StealthUpdate", StealthUpdateModuleData.Parse },
            { "StickyBombUpdate", StickyBombUpdateModuleData.Parse },
            { "StrafeAreaUpdate", StrafeAreaUpdateModuleData.Parse },
            { "StructureCollapseUpdate", StructureCollapseUpdateModuleData.Parse },
            { "StructureToppleUpdate", StructureToppleUpdateModuleData.Parse },
            { "SupplyCenterDockUpdate", SupplyCenterDockUpdateModuleData.Parse },
            { "SupplyCenterProductionExitUpdate", SupplyCenterProductionExitUpdateModuleData.Parse },
            { "SupplyWarehouseDockUpdate", SupplyWarehouseDockUpdateModuleData.Parse },
            { "TemporarilyDefectUpdate", TemporarilyDefectUpdateModuleData.Parse },
            { "TensileFormationUpdate", TensileFormationUpdateModuleData.Parse },
            { "ThreatFinderUpdate", ThreatFinderUpdateModuleData.Parse },
            { "ToppleUpdate", ToppleUpdateModuleData.Parse },
            { "WallUpgradeUpdate", WallUpgradeUpdateModuleData.Parse },
            { "WaveGuideUpdate", WaveGuideUpdateModuleData.Parse },
            { "WeaponBonusUpdate", WeaponBonusUpdateModuleData.Parse },
            { "WeaponModeSpecialPowerUpdate", WeaponModeSpecialPowerUpdateModuleData.Parse },

            // Update/AIUpdate
            { "AIGateUpdate", AIGateUpdateModuleData.Parse },
            { "AISpecialPowerUpdate", AISpecialPowerUpdateModuleData.Parse },
            { "AIUpdateInterface", AIUpdateModuleData.Parse },
            { "AnimalAIUpdate", AnimalAIUpdateModuleData.Parse },
            { "AssaultTransportAIUpdate", AssaultTransportAIUpdateModuleData.Parse },
            { "ChinookAIUpdate", ChinookAIUpdateModuleData.Parse },
            { "DeliverPayloadAIUpdate", DeliverPayloadAIUpdateModuleData.Parse },
            { "DeployStyleAIUpdate", DeployStyleAIUpdateModuleData.Parse },
            { "DozerAIUpdate", DozerAIUpdateModuleData.Parse },
            { "FoundationAIUpdate", FoundationAIUpdateModuleData.Parse },
            { "GiantBirdAIUpdate", GiantBirdAIUpdateModuleData.Parse },
            { "HackInternetAIUpdate", HackInternetAIUpdateModuleData.Parse },
            { "HordeAIUpdate", HordeAIUpdateModuleData.Parse },
            { "HordeWorkerAIUpdate", HordeWorkerAIUpdateModuleData.Parse },
            { "JetAIUpdate", JetAIUpdateModuleData.Parse },
            { "MissileAIUpdate", MissileAIUpdateModuleData.Parse },
            { "RailedTransportAIUpdate", RailedTransportAIUpdateModuleData.Parse },
            { "SiegeAIUpdate", SiegeAIUpdateModuleData.Parse },
            { "SupplyTruckAIUpdate", SupplyTruckAIUpdateModuleData.Parse },
            { "TransportAIUpdate", TransportAIUpdateModuleData.Parse },
            { "WanderAIUpdate", WanderAIUpdateModuleData.Parse },
            { "WorkerAIUpdate", WorkerAIUpdateModuleData.Parse },

            // Update/SpecialAbilityUpdate
            { "FlingPassengerSpecialAbilityUpdate", FlingPassengerSpecialAbilityUpdateModuleData.Parse },
            { "HeroModeSpecialAbilityUpdate", HeroModeSpecialAbilityUpdateModuleData.Parse },
            { "ModelConditionSpecialAbilityUpdate", ModelConditionSpecialAbilityUpdateModuleData.Parse },
            { "SpecialAbilityUpdate", SpecialAbilityUpdateModuleData.Parse },
            { "SummonReplacementSpecialAbilityUpdate", SummonReplacementSpecialAbilityUpdateModuleData.Parse },
            { "TeleportSpecialAbilityUpdate", TeleportSpecialAbilityUpdateModuleData.Parse },
            { "ToggleDeploySpecialAbilityUpdate", ToggleDeploySpecialAbilityUpdateModuleData.Parse },
            { "ToggleHiddenSpecialAbilityUpdate", ToggleHiddenSpecialAbilityUpdateModuleData.Parse },
            { "ToggleMountedSpecialAbilityUpdate", ToggleMountedSpecialAbilityUpdateModuleData.Parse },
            { "WeaponFireSpecialAbilityUpdate", WeaponFireSpecialAbilityUpdateModuleData.Parse },


            // Upgrade
            { "AllowBannerSpawnUpgrade", AllowBannerSpawnUpgradeModuleData.Parse },
            { "ArmorUpgrade", ArmorUpgradeModuleData.Parse },
            { "AttributeModifierUpgrade", AttributeModifierUpgradeModuleData.Parse },
            { "AudioLoopUpgrade", AudioLoopUpgradeModuleData.Parse },
            { "BaseUpgrade", BaseUpgradeModuleData.Parse },
            { "BuildableHeroListUpgrade", BuildableHeroListUpgradeModuleData.Parse },
            { "CastleUpgrade", CastleUpgradeModuleData.Parse },
            { "CommandPointsUpgrade", CommandPointsUpgradeModuleData.Parse },
            { "CommandSetUpgrade", CommandSetUpgradeModuleData.Parse },
            { "CostModifierUpgrade", CostModifierUpgradeModuleData.Parse },
            { "DoCommandUpgrade", DoCommandUpgradeModuleData.Parse },
            { "ExperienceScalarUpgrade", ExperienceScalarUpgradeModuleData.Parse },
            { "GeometryUpgrade", GeometryUpgradeModuleData.Parse },
            { "GrantScienceUpgrade", GrantScienceUpgradeModuleData.Parse },
            { "LevelUpUpgrade", LevelUpUpgradeModuleData.Parse },
            { "LocomotorSetUpgrade", LocomotorSetUpgradeModuleData.Parse },
            { "ModelConditionUpgrade", ModelConditionUpgradeModuleData.Parse },
            { "MaxHealthUpgrade", MaxHealthUpgradeModuleData.Parse },
            { "ObjectCreationUpgrade", ObjectCreationUpgradeModuleData.Parse },
            { "PassengersFireUpgrade", PassengersFireUpgradeModuleData.Parse },
            { "PowerPlantUpgrade", PowerPlantUpgradeModuleData.Parse },
            { "RadarUpgrade", RadarUpgradeModuleData.Parse },
            { "RemoveUpgradeUpgrade", RemoveUpgradeUpgradeModuleData.Parse },
            { "ReplaceObjectUpgrade", ReplaceObjectUpgradeModuleData.Parse },
            { "ReplaceSelfUpgrade", ReplaceSelfUpgradeModuleData.Parse },
            { "SpellRechargeModifierUpgrade", SpellRechargeModifierUpgradeModuleData.Parse },
            { "StatusBitsUpgrade", StatusBitsUpgradeModuleData.Parse },
            { "StealthUpgrade", StealthUpgradeModuleData.Parse },
            { "SubObjectsUpgrade", SubObjectsUpgradeModuleData.Parse },
            { "TooltipUpgrade", ToolTipUpgradeModuleData.Parse },
            { "UnpauseSpecialPowerUpgrade", UnpauseSpecialPowerUpgradeModuleData.Parse },
            { "WeaponBonusUpgrade", WeaponBonusUpgradeModuleData.Parse },
            { "WeaponSetUpgrade", WeaponSetUpgradeModuleData.Parse },
        };

        internal virtual BehaviorModule CreateModule(GameObject gameObject, GameContext context) => null; // TODO: Make this abstract.
    }
}
