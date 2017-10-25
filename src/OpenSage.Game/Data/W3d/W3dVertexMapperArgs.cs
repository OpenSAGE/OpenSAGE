using System.IO;
using static OpenSage.Data.Utilities.ParseUtility;

namespace OpenSage.Data.W3d
{
    // See MAPPERS.TXT in the W3DView folder for more details.
    public sealed class W3dVertexMapperArgs
    {
        public float UPerSec;
        public float VPerSec;

        public float UScale = 1;
        public float VScale = 1;

        public float FPS = 1;

        /// <summary>
        /// 0 = width 1, 1 = width 2, 2 = width 4.
        /// The default means animate using a texture divided up into quarters.
        /// </summary>
        public int Log1Width = 1;

        public int Log2Width = 1;

        /// <summary>
        /// The last frame to use.
        /// </summary>
        public int Last;

        /// <summary>
        /// Units are hertz. 1 = 1 rotation per second.
        /// </summary>
        public float Speed = 0.1f;

        public float UCenter;
        public float VCenter;

        public float UAmp = 1;
        public float UFreq = 1;
        public float UPhase;

        public float VAmp = 1;
        public float VFreq = 1;
        public float VPhase;

        public float UStep;
        public float VStep;
        public float StepsPerSecond;

        /// <summary>
        /// In Hertz. 1 = 1 rotate per second. DEFAULT = 0.0
        /// </summary>
        public float BumpRotation;

        /// <summary>
        /// Scale factor applied to the bumps. DEFAULT = 1.0
        /// </summary>
        public float BumpScale = 1;

        public static W3dVertexMapperArgs Parse(string value)
        {
            var result = new W3dVertexMapperArgs();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            var splitMapperArgs0 = value.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var mapperArg in splitMapperArgs0)
            {
                var splitMapperArg = mapperArg.Split('=');

                var mapperArgName = splitMapperArg[0].Trim();

                var mapperArgValue = splitMapperArg[1].Trim();
                if (mapperArgValue.Contains(";"))
                {
                    // ';' indicates a comment
                    mapperArgValue = mapperArgValue.Substring(0, mapperArgValue.IndexOf(';')).Trim();
                }

                mapperArgValue = mapperArgValue.TrimEnd('f').Replace("..", ".");

                switch (mapperArgName)
                {
                    case "UPerSec":
                        TryParseFloat(mapperArgValue, out result.UPerSec);
                        break;

                    case "VPerSec":
                        TryParseFloat(mapperArgValue, out result.VPerSec);
                        break;

                    case "UScale":
                        TryParseFloat(mapperArgValue, out result.UScale);
                        break;

                    case "VScale":
                        TryParseFloat(mapperArgValue, out result.VScale);
                        break;

                    case "FPS":
                        TryParseFloat(mapperArgValue, out result.FPS);
                        break;

                    case "Log1Width":
                        result.Log1Width = int.Parse(mapperArgValue);
                        break;

                    case "Log2Width":
                        result.Log2Width = int.Parse(mapperArgValue);
                        break;

                    case "Last":
                        int.TryParse(mapperArgValue, out result.Last);
                        break;

                    case "Speed":
                        TryParseFloat(mapperArgValue, out result.Speed);
                        break;

                    case "UCenter":
                        TryParseFloat(mapperArgValue, out result.UCenter);
                        break;

                    case "VCenter":
                        TryParseFloat(mapperArgValue, out result.VCenter);
                        break;

                    case "UAmp":
                        TryParseFloat(mapperArgValue, out result.UAmp);
                        break;

                    case "UFreq":
                        TryParseFloat(mapperArgValue, out result.UFreq);
                        break;

                    case "UPhase":
                        TryParseFloat(mapperArgValue, out result.UPhase);
                        break;

                    case "VAmp":
                        TryParseFloat(mapperArgValue, out result.VAmp);
                        break;

                    case "VFreq":
                        TryParseFloat(mapperArgValue, out result.VFreq);
                        break;

                    case "VPhase":
                        TryParseFloat(mapperArgValue, out result.VPhase);
                        break;

                    case "BumpRotation":
                        TryParseFloat(mapperArgValue, out result.BumpRotation);
                        break;

                    case "BumpScale":
                        TryParseFloat(mapperArgValue, out result.BumpScale);
                        break;

                    case "UStep":
                        TryParseFloat(mapperArgValue, out result.UStep);
                        break;

                    case "VStep":
                        TryParseFloat(mapperArgValue, out result.VStep);
                        break;

                    case "SPS":
                        TryParseFloat(mapperArgValue, out result.StepsPerSecond);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown mapper arg. Name = {mapperArgName}, Value = {splitMapperArg[1]}");
                }
            }

            return result;
        }
    }
}
