using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSage.Terrain
{
    public interface IHeightMap
    {
        public float GetHeight(float x, float y);
        public int MaxXCoordinate { get; }
        public int MaxYCoordinate { get; }
    }
}