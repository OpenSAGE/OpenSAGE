using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.LowLevel.Graphics2D
{
    public struct RawLineF
    {
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
        public float Thickness;

        public RawLineF(float x1, float y1, float x2, float y2, float thickness)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Thickness = thickness;
        }
    }
}
