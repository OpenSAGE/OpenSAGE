﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CommandButtonHuntUpdate : UpdateModule
    {
        // TODO
    }

    /// <summary>
    /// Allows this object to hunt using a particular ability or command via scripts. AI only.
    /// </summary>
    public sealed class CommandButtonHuntUpdateModuleData : UpdateModuleData
    {
        internal static CommandButtonHuntUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CommandButtonHuntUpdateModuleData> FieldParseTable = new IniParseTable<CommandButtonHuntUpdateModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CommandButtonHuntUpdate();
        }
    }
}
