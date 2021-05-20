﻿using System.Diagnostics;
using System.IO;

namespace OpenSage.Data.Map
{
    [DebuggerDisplay("Team '{GetName()}'")]
    public sealed class Team
    {
        public AssetPropertyCollection Properties { get; internal set; }

        internal static Team Parse(BinaryReader reader, MapParseContext context)
        {
            return new Team
            {
                Properties = AssetPropertyCollection.Parse(reader, context)
            };
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            Properties.WriteTo(writer, assetNames);
        }

        private string GetName() => (string)Properties["teamName"].Value;
    }
}
