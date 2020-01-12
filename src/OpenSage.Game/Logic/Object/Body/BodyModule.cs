using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class BodyModule : BehaviorModule
    {
        // TODO: This could use a smaller fixed point type.
        // TODO: This shouldn't be publicly settable.
        public decimal Health { get; set; }

        public abstract decimal MaxHealth { get; }

        public virtual void SetInitialHealth(float multiplier) { }
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
