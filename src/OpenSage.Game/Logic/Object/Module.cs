using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class ModuleData
    {
        internal static ModuleDataContainer ParseModule<T>(IniParser parser, Dictionary<string, Func<IniParser, T>> moduleParseTable, ModuleInheritanceMode inheritanceMode)
            where T : ModuleData
        {
            var moduleType = parser.GetNextToken();
            var tag = parser.GetNextToken();

            if (!moduleParseTable.TryGetValue(moduleType.Text, out var moduleParser))
            {
                throw new IniParseException($"Unknown module type: {moduleType.Text}", moduleType.Position);
            }

            var result = moduleParser(parser);

            return new ModuleDataContainer(tag.Text, result, inheritanceMode, false);
        }

        public virtual ModuleKinds ModuleKinds => ModuleKinds.None;
    }

    [Flags]
    public enum ModuleKinds
    {
        None         = 0x0,
        Body         = 0x1,
        ClientUpdate = 0x2,
        Collide      = 0x4,
        Contain      = 0x8,
        Create       = 0x10,
        Damage       = 0x20,
        Die          = 0x40,
        Draw         = 0x80,
        SpecialPower = 0x100,
        Update       = 0x200,
        Upgrade      = 0x400,
    }

    public readonly struct ModuleDataContainer
    {
        public readonly string Tag;
        public readonly ModuleData Data;
        public readonly ModuleInheritanceMode InheritanceMode;
        public readonly bool Inherited;

        internal ModuleDataContainer(string tag, ModuleData data, ModuleInheritanceMode inheritanceMode, bool inherited)
        {
            Tag = tag;
            Data = data;
            InheritanceMode = inheritanceMode;
            Inherited = inherited;
        }

        internal ModuleDataContainer WithInherited()
        {
            return new ModuleDataContainer(Tag, Data, InheritanceMode, true);
        }
    }

    public enum ModuleInheritanceMode
    {
        /// <summary>
        /// This module is inherited, but if the inheriting object defines a module
        /// with the same <see cref="ModuleKinds"/>, then this module is removed.
        /// </summary>
        Default,

        /// <summary>
        /// This module is always inherited unless explicitly removed with RemoveModule.
        /// </summary>
        AlwaysInherit,

        /// <summary>
        /// This module is inherited, but if the inheriting object defines a module
        /// with the exact same class, then this module is removed.
        /// </summary>
        OverrideableByLikeKind,
    }
}
