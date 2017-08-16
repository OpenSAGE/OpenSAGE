using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dLight
    {
        public W3dLightFlags Attributes { get; private set; }

        /// <summary>
        /// Old exclusion bit deprecated
        /// </summary>
        public uint Unused { get; private set; }

        public W3dRgb Ambient { get; private set; }
        public W3dRgb Diffuse { get; private set; }
        public W3dRgb Specular { get; private set; }

        public float Intensity { get; private set; }

        public static W3dLight Parse(BinaryReader reader)
        {
            return new W3dLight
            {
                Attributes = (W3dLightFlags) reader.ReadUInt32(),
                Unused = reader.ReadUInt32(),

                Ambient = W3dRgb.Parse(reader),
                Diffuse = W3dRgb.Parse(reader),
                Specular = W3dRgb.Parse(reader),

                Intensity = reader.ReadSingle()
            };
        }
    }
}
