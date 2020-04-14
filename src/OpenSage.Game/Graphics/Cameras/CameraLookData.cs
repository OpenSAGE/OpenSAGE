using System.Numerics;
using System.Text;

namespace OpenSage.Graphics.Cameras
{


    class CameraLookData
    {
        public Vector3 newPosition { get; set; }

        public Vector3 targetPosition { get; set; }

        public Vector3 lookDirection { get; set; }

        public Vector3 terrainPosition { get; set; }

        public float pitch { get; set; }

        public float zoom { get; set; }

        public float intersect { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (System.Reflection.PropertyInfo property in this.GetType().GetProperties())
            {
                sb.Append(property.Name);
                sb.Append(": ");
                if (property.GetIndexParameters().Length > 0)
                {
                    sb.Append("Indexed Property cannot be used");
                }
                else
                {
                    sb.Append(property.GetValue(this, null));
                }

                sb.Append(System.Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}