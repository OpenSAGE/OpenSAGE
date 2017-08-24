using System;

namespace OpenSage.Data.Ini
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AddedInAttribute : Attribute
    {
        public SageGame Game { get; }

        public AddedInAttribute(SageGame game)
        {
            Game = game;
        }
    }

    public enum SageGame
    {
        CncGenerals,
        CncGeneralsZeroHour
    }
}
