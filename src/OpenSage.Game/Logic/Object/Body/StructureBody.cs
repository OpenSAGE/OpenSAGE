﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class StructureBody : ActiveBody
    {
        private uint _unknown;

        internal StructureBody(GameObject gameObject, StructureBodyModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32(ref _unknown);
        }
    }

    /// <summary>
    /// Used by objects with STRUCTURE and IMMOBILE KindOfs defined.
    /// </summary>
    public sealed class StructureBodyModuleData : ActiveBodyModuleData
    {
        internal static new StructureBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StructureBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<StructureBodyModuleData>());

        internal override BodyModule CreateBodyModule(GameObject gameObject)
        {
            return new StructureBody(gameObject, this);
        }
    }
}
