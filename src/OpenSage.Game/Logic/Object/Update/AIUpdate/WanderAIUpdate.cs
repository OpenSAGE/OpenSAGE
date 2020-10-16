﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows this object to move randomly about its point of origin using a SET_WANDER locomotor.
    /// </summary>
    public sealed class WanderAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static WanderAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<WanderAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<WanderAIUpdateModuleData>());
    }
}
