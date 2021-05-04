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

        public virtual ModuleKind ModuleKind => ModuleKind.None;
    }

    public enum ModuleKind
    {
        None,
        Body,
        ClientUpdate,
        Collide,
        Contain,
        Create,
        Damage,
        Die,
        Draw,
        SpecialPower,
        Update,
        Upgrade,
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
        /// with the same <see cref="ModuleKind"/>, then this module is removed.
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
