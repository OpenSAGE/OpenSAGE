using System.IO;

namespace OpenSage.Data.W3d
{
    // See MAPPERS.TXT in the W3DView folder for more details.
    public sealed class W3dVertexMapperArgs
    {
        public float UPerSec;
        public float VPerSec;

        public float UScale;
        public float VScale;

        public float FPS;

        /// <summary>
        /// 0 = width 1, 1 = width 2, 2 = width 4.
        /// The default means animate using a texture divided up into quarters.
        /// </summary>
        public int Log1Width;

        /// <summary>
        /// The last frame to use.
        /// </summary>
        public int Last;

        public static W3dVertexMapperArgs Parse(string value)
        {
            var result = new W3dVertexMapperArgs();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            var splitMapperArgs0 = value.Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var mapperArg in splitMapperArgs0)
            {
                var splitMapperArg = mapperArg.Split('=');

                var mapperArgName = splitMapperArg[0];

                var mapperArgValue = splitMapperArg[1];
                if (mapperArgValue.Contains(";"))
                {
                    // ';' indicates a comment
                    mapperArgValue = mapperArgValue.Substring(0, mapperArgValue.IndexOf(';'));
                }

                switch (splitMapperArg[0])
                {
                    case "UPerSec":
                        result.UPerSec = float.Parse(mapperArgValue);
                        break;

                    case "VPerSec":
                        result.VPerSec = float.Parse(mapperArgValue);
                        break;

                    case "UScale":
                        result.UScale = float.Parse(mapperArgValue);
                        break;

                    case "VScale":
                        result.VScale = float.Parse(mapperArgValue);
                        break;

                    case "FPS":
                        result.FPS = float.Parse(mapperArgValue);
                        break;

                    case "Log1Width":
                        result.Log1Width = int.Parse(mapperArgValue);
                        break;

                    case "Last":
                        result.Last = int.Parse(mapperArgValue);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown mapper arg. Name = {mapperArgName}, Value = {splitMapperArg[1]}");
                }
            }

            return result;
        }
    }
}
