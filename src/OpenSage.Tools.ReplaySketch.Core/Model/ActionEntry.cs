using System.Text.Json.Serialization;

namespace OpenSage.Tools.ReplaySketch.Model;

public sealed class ActionEntry
{
    public string Label { get; set; }
    public ActionType Type { get; set; }

    /// <summary>
    /// Null for actions that do not target a map position (e.g. <see cref="ActionType.RecruitBasicUnit"/>).
    /// </summary>
    public PositionSpec? Position { get; set; }

    public TimingConfig Timing { get; set; }

    [JsonConstructor]
    public ActionEntry(string label, ActionType type, PositionSpec? position, TimingConfig timing)
    {
        Label = label;
        Type = type;
        Position = position;
        Timing = timing;
    }
}
