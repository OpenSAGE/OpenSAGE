using System.Collections.Generic;
using OpenSage.Tools.ReplaySketch.Services;

namespace OpenSage.Tools.ReplaySketch.Model;

public sealed class ReplayScenario
{
    /// <summary>
    /// Map file path as stored in the replay metadata,
    /// e.g. <c>"maps/alpine assault/alpine assault.map"</c>.
    /// </summary>
    public string MapPath { get; set; } = string.Empty;

    /// <summary>
    /// World-unit radius used as "1 base width" for landmark-relative positions.
    /// Roughly the footprint radius of a Command Center (~120 world units).
    /// </summary>
    public float BaseRadiusWorldUnits { get; set; } = 120f;

    public List<PlayerSlotConfig> Players { get; set; } = new();

    // -----------------------------------------------------------------
    // Factory
    // -----------------------------------------------------------------

    /// <summary>
    /// Loads the Alpine Assault USA vs GLA scenario from its embedded JSON profile.
    /// Edit <c>Profiles/alpine-assault-usa-vs-gla.json</c> to change actions or timing.
    /// </summary>
    public static ReplayScenario CreateAlpineAssaultUSAvGLA() =>
        ScenarioProfileLoader.LoadEmbedded("alpine-assault-usa-vs-gla.json");
}
