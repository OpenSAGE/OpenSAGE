#nullable enable

using System.Diagnostics;
using System.IO;

namespace OpenSage.Data.Map;

// Equivalent to TeamsInfo in C++
[DebuggerDisplay("Team '{Name}'")]
public sealed class Team
{
    public Team() { }
    public Team(AssetPropertyCollection properties)
    {
        Properties = properties;
    }

    public AssetPropertyCollection Properties { get; internal set; } = [];

    internal static Team Parse(BinaryReader reader, MapParseContext context)
    {
        var properties = AssetPropertyCollection.Parse(reader, context);
        return new Team(properties);
    }

    internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
    {
        Properties.WriteTo(writer, assetNames);
    }

    public string? Name
    {
        get => Properties[TeamKeys.Name].GetAsciiString();
        set
        {
            Properties.TryGetValue(TeamKeys.Name, out var property);
            if (property == null)
            {
                Properties.AddAsciiString(TeamKeys.Name, value ?? string.Empty);
            }
            else
            {
                property.UpdateValue(value ?? string.Empty);
            }
        }
    }

    public bool IsSingleton => Properties[TeamKeys.IsSingleton].GetBoolean() ?? false;
    public string? Owner
    {
        get => Properties[TeamKeys.Owner].GetAsciiString();
        set
        {
            Properties.TryGetValue(TeamKeys.Owner, out var property);
            if (property == null)
            {
                Properties.AddAsciiString(TeamKeys.Owner, value ?? string.Empty);
            }
            else
            {
                property.UpdateValue(value ?? string.Empty);
            }
        }
    }
}

public static class TeamKeys
{
    public const string Name = "teamName";
    public const string IsSingleton = "teamIsSingleton";
    public const string Owner = "teamOwner";
}
