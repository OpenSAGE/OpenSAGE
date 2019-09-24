﻿using OpenSage.Data.Ini;
using Veldrid;

namespace OpenSage.Input
{
    public sealed class CommandMap : BaseAsset
    {
        internal static CommandMap Parse(IniParser parser)
        {
           return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("CommandMap", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<CommandMap> FieldParseTable = new IniParseTable<CommandMap>
        {
            { "Key", (parser, x) => x.Key = parser.ParseEnum<Key>() },
            { "Transition", (parser, x) => x.Transition = parser.ParseEnum<KeyTransition>() },
            { "Modifiers", (parser, x) => x.Modifiers = parser.ParseEnum<KeyModifiers>() },
            { "UseableIn", (parser, x) => x.UseableIn = parser.ParseEnumFlags<CommandMapUsabilityFlags>() },
            { "Category", (parser, x) => x.Category = parser.ParseEnum<CommandMapCategory>() },
            { "Description", (parser, x) => x.Description = parser.ParseLocalizedStringKey() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() }
        };

        public Key Key { get; private set; }
        public KeyTransition Transition { get; private set; }
        public KeyModifiers Modifiers { get; private set; }
        public CommandMapUsabilityFlags UseableIn { get; private set; }
        public CommandMapCategory Category { get; private set; }
        public string Description { get; private set; }
        public string DisplayName { get; private set; }
    }
}
