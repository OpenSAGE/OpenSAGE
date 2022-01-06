using System;
using System.Collections.Generic;
using FixedMath.NET;
using ImGuiNET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class BodyModule : BehaviorModule
    {
        public Fix64 Health { get; internal set; }

        public abstract Fix64 MaxHealth { get; internal set; }

        public Fix64 HealthPercentage => MaxHealth != Fix64.Zero ? Health / MaxHealth : Fix64.Zero;

        public virtual void SetInitialHealth(float multiplier) { }

        public virtual void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, TimeInterval time) { }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            var unknownFloat = 1.0f;
            reader.PersistSingle("UnknownFloat", ref unknownFloat);
            if (unknownFloat != 1.0f)
            {
                throw new InvalidStateException();
            }
        }

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

        internal sealed override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            throw new InvalidOperationException();
        }

        internal virtual BodyModule CreateBodyModule(GameObject gameObject) => null; // TODO: Make this abstract.
    }
}
