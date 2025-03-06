#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Logic.Map;

namespace OpenSage.Data.Map;

public sealed class PolygonTriggers : Asset
{
    public const string AssetName = "PolygonTriggers";

    public List<PolygonTrigger> Triggers { get; init; } = [];

    internal static PolygonTriggers Parse(BinaryReader reader, MapParseContext context)
    {
        return ParseAsset(reader, context, version =>
        {
            var numTriggers = reader.ReadUInt32();
            var triggers = new List<PolygonTrigger>((int)numTriggers);
            var maxTriggerId = 0u;

            for (var i = 0; i < numTriggers; i++)
            {
                var area = PolygonTrigger.Parse(reader, version);

                if (area != null)
                {
                    triggers.Add(area);
                    maxTriggerId = Math.Max(maxTriggerId, area.TriggerId);
                }
            }

            // Create default water area for version 1 maps
            if (version == 1)
            {
                var waterArea = PolygonTrigger.CreateDefaultWaterArea(maxTriggerId + 1);
                triggers.Add(waterArea);
            }

            return new PolygonTriggers
            {
                Triggers = triggers
            };
        });
    }

    internal void WriteTo(BinaryWriter writer)
    {
        WriteAssetTo(writer, () =>
        {
            writer.Write((uint)Triggers.Count);

            foreach (var trigger in Triggers)
            {
                trigger.WriteTo(writer, Version);
            }
        });
    }

    internal PolygonTrigger GetPolygonTriggerById(uint id)
    {
        var trigger = Triggers.Find(trigger => trigger.TriggerId == id) ?? throw new InvalidOperationException();
        return trigger;
    }

    internal PolygonTrigger GetPolygonTriggerByName(string name)
    {
        var trigger = Triggers.Find(trigger => trigger.Name == name) ?? throw new InvalidOperationException();
        return trigger;
    }
}
