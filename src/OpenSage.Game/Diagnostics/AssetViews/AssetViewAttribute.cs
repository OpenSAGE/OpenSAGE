using System;

namespace OpenSage.Diagnostics.AssetViews
{
    public sealed class AssetViewAttribute : Attribute
    {
        public Type ForType { get; }

        public AssetViewAttribute(Type forType)
        {
            ForType = forType;
        }
    }
}
