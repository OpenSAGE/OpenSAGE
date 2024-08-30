using System;
using System.Collections.Generic;
using FixedMath.NET;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;

namespace OpenSage.Logic.Object
{
    public abstract class BodyModule : BehaviorModule
    {
        private float _armorDamageScalar;

        protected GameObject GameObject { get; }

        protected BodyModule(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public Fix64 Health { get; internal set; }

        public abstract Fix64 MaxHealth { get; internal set; }

        public Fix64 HealthPercentage => MaxHealth != Fix64.Zero ? Health / MaxHealth : Fix64.Zero;

        public virtual void SetInitialHealth(float multiplier) { }

        public virtual void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, GameObject damageDealer) { }

        public virtual void Heal(Fix64 amount) { }

        public virtual void Heal(Fix64 amount, GameObject healer) { }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistSingle(ref _armorDamageScalar); // was roughly 0.9 after changing to hold the line
        }

        private DamageType _inspectorDamageType = DamageType.Explosion;
        private float _inspectorDamageAmount;
        private DeathType _inspectorDeathType = DeathType.Normal;

        internal override void DrawInspector()
        {
            var maxHealth = (float) MaxHealth;
            if (ImGui.InputFloat("MaxHealth", ref maxHealth))
            {
                MaxHealth = (Fix64) maxHealth;
            }

            var health = (float) Health;
            if (ImGui.InputFloat("Health", ref health))
            {
                Health = (Fix64) health;
            }

            ImGui.Separator();

            ImGuiUtility.ComboEnum("Damage Type", ref _inspectorDamageType);
            ImGui.InputFloat("Damage Amount", ref _inspectorDamageAmount);
            ImGuiUtility.ComboEnum("Death Type", ref _inspectorDeathType);
            if (ImGui.Button("Apply Damage"))
            {
                GameObject.DoDamage(_inspectorDamageType, (Fix64) _inspectorDamageAmount, _inspectorDeathType, null);
            }
        }
    }

    public abstract class BodyModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Body;

        internal static ModuleDataContainer ParseBody(IniParser parser, ModuleInheritanceMode inheritanceMode) => ParseModule(parser, BodyParseTable, inheritanceMode);

        private static readonly Dictionary<string, Func<IniParser, BodyModuleData>> BodyParseTable = new Dictionary<string, Func<IniParser, BodyModuleData>>
        {
            { "ActiveBody", ActiveBodyModuleData.Parse },
            { "DelayedDeathBody", DelayedDeathBodyModuleData.Parse },
            { "DetachableRiderBody", DetachableRiderBodyModuleData.Parse },
            { "FreeLifeBody", FreeLifeBodyModuleData.Parse },
            { "HighlanderBody", HighlanderBodyModuleData.Parse },
            { "HiveStructureBody", HiveStructureBodyModuleData.Parse },
            { "ImmortalBody", ImmortalBodyModuleData.Parse },
            { "InactiveBody", InactiveBodyModuleData.Parse },
            { "PorcupineFormationBodyModule", PorcupineFormationBodyModuleData.Parse },
            { "RespawnBody", RespawnBodyModuleData.Parse },
            { "StructureBody", StructureBodyModuleData.Parse },
            { "SymbioticStructuresBody", SymbioticStructuresBodyModuleData.Parse },
            { "UndeadBody", UndeadBodyModuleData.Parse },
        };
    }
}
