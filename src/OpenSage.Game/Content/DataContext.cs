using System;
using System.Collections.Generic;
using OpenSage.Graphics.Animation;

namespace OpenSage.Content
{
    /// <summary>
    /// Eventually all game data will live in this hierarchy of objects,
    /// with global or map-specific scope.
    /// </summary>
    public sealed class DataContext
    {
        public Dictionary<string, Animation> Animations { get; } = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
    }
}
