using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    public abstract class BodyModule : BehaviorModule
    {
        public Fix64 Health { get; protected set; }

        public abstract Fix64 MaxHealth { get; }

        public Fix64 HealthPercentage => Health / MaxHealth;

        public virtual void SetInitialHealth(float multiplier) { }

        public virtual void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType) { }
    }

    public abstract class BodyModuleData : BehaviorModuleData
    {
        internal static BodyModuleData ParseBody(IniParser parser) => ParseModule(parser, BodyParseTable);

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

        internal sealed override BehaviorModule CreateModule(GameObject gameObject)
        {
            throw new InvalidOperationException();
        }

        internal virtual BodyModule CreateBodyModule(GameObject gameObject) => null; // TODO: Make this abstract.
    }
}
