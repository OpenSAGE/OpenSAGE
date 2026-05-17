using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSage.Tools.ReplaySketch.Model;

namespace OpenSage.Tools.ReplaySketch.Services;

/// <summary>
/// Loads <see cref="ReplayScenario"/> objects from JSON profile files.
/// </summary>
public static class ScenarioProfileLoader
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Loads a <see cref="ReplayScenario"/> from the embedded profile resource with the given name.
    /// </summary>
    /// <param name="resourceName">
    /// Bare filename of the profile, e.g. <c>"alpine-assault-usa-vs-gla.json"</c>.
    /// The file must exist under the <c>Profiles/</c> folder in this assembly.
    /// </param>
    public static ReplayScenario LoadEmbedded(string resourceName)
    {
        var assembly = typeof(ScenarioProfileLoader).Assembly;
        var fullName = $"OpenSage.Tools.ReplaySketch.Core.Profiles.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(fullName)
            ?? throw new InvalidOperationException(
                $"Embedded profile '{fullName}' not found. " +
                $"Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");

        var scenario = JsonSerializer.Deserialize<ReplayScenario>(stream, s_options)
            ?? throw new InvalidOperationException($"Deserialization of '{fullName}' returned null.");

        return scenario;
    }

    /// <summary>
    /// Loads a <see cref="ReplayScenario"/> from a file on disk.
    /// Supports JSON with <c>//</c> and <c>/* */</c> comments and trailing commas.
    /// </summary>
    public static ReplayScenario LoadFromFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<ReplayScenario>(stream, s_options)
            ?? throw new InvalidOperationException($"Deserialization of '{filePath}' returned null.");
    }
}
