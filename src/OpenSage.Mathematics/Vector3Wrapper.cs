using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Mathematics
{
    public class Vector3Wrapper
    {
        private float x;
        private float y;
        private float z;

        public float X
        {
            get
            {
                return x;
            }
            set
            {
                this.x = value;
                this.Vector = new Vector3(value, this.Y, this.Z);
            }
        }

        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                this.y = value;
                this.Vector = new Vector3(this.X, value, this.Z);
            }
        }

        public float Z
        {
            get
            {
                return z;
            }
            set
            {
                this.z = value;
                this.Vector = new Vector3(this.X, this.Y, value);
            }
        }

        public Vector3 Vector {get; private set;}

        public Vector3Wrapper()
        {
            this.Vector = new Vector3();
        }

        public Vector3Wrapper(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Vector = new Vector3(x, y, z);
        }

        public Vector3Wrapper(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
            this.Vector = vector;
        }
    }
}
