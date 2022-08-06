using System;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    // See MAPPERS.TXT in the W3DView folder for more details.
    public sealed class W3dVertexMapperArgs : W3dChunk
    {
        public override W3dChunkType ChunkType { get; }

        public W3dVertexMapperArgs(W3dChunkType chunkType)
        {
            ChunkType = chunkType;
        }

        public string RawValue;

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
        public float Offset;
        public float Axis;
        public float UOffset;
        public float VOffset;
        public float ClampFix;
        public float UseReflect;
        public float Period;
        public float VPerScale;

        /// <summary>
        /// In Hertz. 1 = 1 rotate per second. DEFAULT = 0.0
        /// </summary>
        public float BumpRotation;

        /// <summary>
        /// Scale factor applied to the bumps. DEFAULT = 1.0
        /// </summary>
        public float BumpScale = 1;

        internal static W3dVertexMapperArgs Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
        {
            return ParseChunk(reader, context, header =>
            {
                var value = reader.ReadFixedLengthString((int) header.ChunkSize);

                var result = new W3dVertexMapperArgs(chunkType)
                {
                    RawValue = value
                };

                if (string.IsNullOrEmpty(value))
                {
                    return result;
                }

                var splitMapperArgs0 = value.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (var mapperArg in splitMapperArgs0)
                {
                    var splitByDash = mapperArg.Contains("-"); // EnB asset ("pu09a.w3d") contains erroneous mapping. - is used instead of =. 
                    var splitByEquals = mapperArg.Contains("=");

                    var splitValue = (splitByEquals) ? '=' : '-';
                    var splitMapperArg = mapperArg.Split(splitValue);

                    var mapperArgName = splitMapperArg[0].Trim();

                    // enb contains assets that use ":" as comments start as well as some that contain no value or comment dilimeter.
                    if (!mapperArgName.Contains(";") && !mapperArgName.Contains(":") && splitMapperArg.Length == 2) 
                    {
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
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UPerSec);
                                break;

                            case "VPerSec":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VPerSec);
                                break;

                            case "UScale":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UScale);
                                break;

                            case "VScale":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VScale);
                                break;

                            case "FPS":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.FPS);
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
                                ParseUtility.TryParseFloat(mapperArgValue, out result.Speed);
                                break;

                            case "UCenter":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UCenter);
                                break;

                            case "VCenter":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VCenter);
                                break;

                            case "UAmp":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UAmp);
                                break;

                            case "UFreq":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UFreq);
                                break;

                            case "UPhase":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UPhase);
                                break;

                            case "VAmp":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VAmp);
                                break;

                            case "VFreq":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VFreq);
                                break;

                            case "VPhase":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VPhase);
                                break;

                            case "BumpRotation":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.BumpRotation);
                                break;

                            case "BumpScale":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.BumpScale);
                                break;

                            case "UStep":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UStep);
                                break;

                            case "VStep":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VStep);
                                break;

                            case "SPS":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.StepsPerSecond);
                                break;

                            case "Offset":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.Offset);
                                break;

                            case "Axis":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.Axis);
                                break;

                            case "UOffset":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UOffset);
                                break;

                            case "VOffset":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VOffset);
                                break;

                            case "ClampFix":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.ClampFix);
                                break;

                            case "UseReflect":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UseReflect);
                                break;

                            case "VPreSec":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VPerSec);
                                break;

                            case "Period":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.Period);
                                break;

                            case "UperSec":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.UPerSec);
                                break;

                            case "fps":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.FPS);
                                break;

                            case "VPerScale":
                                ParseUtility.TryParseFloat(mapperArgValue, out result.VPerScale);
                                break;

                            default:
                                throw new InvalidDataException($"Unknown mapper arg. Name = {mapperArgName}, Value = {splitMapperArg[1]}");
                        }
                    }
                }

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            if (RawValue != null)
            {
                writer.WriteFixedLengthString(RawValue, RawValue.Length + 1);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
