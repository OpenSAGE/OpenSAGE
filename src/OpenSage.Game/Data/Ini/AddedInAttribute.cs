using System;

namespace OpenSage.Data.Ini
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
    public sealed class AddedInAttribute : Attribute
    {
        public SageGame Game { get; }

        public AddedInAttribute(SageGame game)
        {
            Game = game;
        }
    }
}
