﻿using OpenSage.Mathematics;

namespace OpenSage.Content
{
    public class LoadOptions
    {
        // TODO: Refactor this, it's not a good API.
        public bool CacheAsset { get; set; } = true;
        public ColorRgb HouseColor { get; set; }
    }
}
