using System.Collections.Generic;

namespace OpenSage.Tools.ReplaySketch.Model;

public sealed class PlayerSlotConfig
{
    public string Name { get; set; }

    /// <summary>
    /// Index into the game's PlayerTemplates list.
    /// USA = 2, GLA = 4 (Generals defaults).
    /// </summary>
    public int FactionIndex { get; set; }

    public sbyte Color { get; set; }

    /// <summary>1-based start position on the map (matches Player_N_Start waypoint naming).</summary>
    public int StartPosition { get; set; }

    /// <summary>1-based team number.</summary>
    public int Team { get; set; }

    public List<ActionEntry> Actions { get; set; } = new();
}
