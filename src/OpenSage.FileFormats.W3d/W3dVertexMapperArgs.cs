using System;
using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <remarks>
/// See MAPPERS.TXT in the W3DView folder for more details.
/// </remarks>
/// <param name="ChunkType"></param>
/// <param name="RawValue"></param>
/// <param name="UPerSec"></param>
/// <param name="VPerSec"></param>
/// <param name="UScale"></param>
/// <param name="VScale"></param>
/// <param name="FPS"></param>
/// <param name="Log1Width">0 = width 1, 1 = width 2, 2 = width 4. The default means animate using a texture divided up into quarters.</param>
/// <param name="Log2Width"></param>
/// <param name="Last">The last frame to use</param>
/// <param name="Speed">Units are hertz. 1 = 1 rotation per second</param>
/// <param name="UCenter"></param>
/// <param name="VCenter"></param>
/// <param name="UAmp"></param>
/// <param name="UFreq"></param>
/// <param name="UPhase"></param>
/// <param name="VAmp"></param>
/// <param name="VFreq"></param>
/// <param name="VPhase"></param>
/// <param name="UStep"></param>
/// <param name="VStep"></param>
/// <param name="StepsPerSecond"></param>
/// <param name="Offset"></param>
/// <param name="Axis"></param>
/// <param name="UOffset"></param>
/// <param name="VOffset"></param>
/// <param name="ClampFix"></param>
/// <param name="UseReflect"></param>
/// <param name="Period"></param>
/// <param name="VPerScale"></param>
/// <param name="BumpRotation">In Hertz. 1 = 1 rotate per second. DEFAULT = 0.0</param>
/// <param name="BumpScale">Scale factor applied to the bumps. DEFAULT = 1.0</param>
public sealed record W3dVertexMapperArgs(
    W3dChunkType ChunkType,
    string? RawValue = null,
    float UPerSec = 0,
    float VPerSec = 0,
    float UScale = 1,
    float VScale = 1,
    float FPS = 1,
    int Log1Width = 1,
    int Log2Width = 1,
    int Last = 0,
    float Speed = 0.1f,
    float UCenter = 0,
    float VCenter = 0,
    float UAmp = 1,
    float UFreq = 1,
    float UPhase = 0,
    float VAmp = 1,
    float VFreq = 1,
    float VPhase = 0,
    float UStep = 0,
    float VStep = 0,
    float StepsPerSecond = 0,
    float Offset = 0,
    float Axis = 0,
    float UOffset = 0,
    float VOffset = 0,
    float ClampFix = 0,
    float UseReflect = 0,
    float Period = 0,
    float VPerScale = 0,
    float BumpRotation = 0,
    float BumpScale = 1) : W3dChunk(ChunkType)
{
    internal static W3dVertexMapperArgs Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
    {
        return ParseChunk(reader, context, header =>
        {
            string value = reader.ReadFixedLengthString((int)header.ChunkSize);
            float uPerSec = 0;
            float vPerSec = 0;
            float uScale = 1;
            float vScale = 1;
            float fps = 1;
            int log1Width = 1;
            int log2Width = 1;
            int last = 0;
            float speed = 0.1f;
            float uCenter = 0;
            float vCenter = 0;
            float uAmp = 1;
            float uFreq = 1;
            float uPhase = 0;
            float vAmp = 1;
            float vFreq = 1;
            float vPhase = 0;
            float uStep = 0;
            float vStep = 0;
            float stepsPerSecond = 0;
            float offset = 0;
            float axis = 0;
            float uOffset = 0;
            float vOffset = 0;
            float clampFix = 0;
            float useReflect = 0;
            float period = 0;
            float vPerScale = 0;
            float bumpRotation = 0;
            float bumpScale = 1;

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
                            ParseUtility.TryParseFloat(mapperArgValue, out uPerSec);
                            break;

                        case "VPerSec":
                            ParseUtility.TryParseFloat(mapperArgValue, out vPerSec);
                            break;

                        case "UScale":
                            ParseUtility.TryParseFloat(mapperArgValue, out uScale);
                            break;

                        case "VScale":
                            ParseUtility.TryParseFloat(mapperArgValue, out vScale);
                            break;

                        case "FPS":
                            ParseUtility.TryParseFloat(mapperArgValue, out fps);
                            break;

                        case "Log1Width":
                            log1Width = int.Parse(mapperArgValue);
                            break;

                        case "Log2Width":
                            log2Width = int.Parse(mapperArgValue);
                            break;

                        case "Last":
                            int.TryParse(mapperArgValue, out last);
                            break;

                        case "Speed":
                            ParseUtility.TryParseFloat(mapperArgValue, out speed);
                            break;

                        case "UCenter":
                            ParseUtility.TryParseFloat(mapperArgValue, out uCenter);
                            break;

                        case "VCenter":
                            ParseUtility.TryParseFloat(mapperArgValue, out vCenter);
                            break;

                        case "UAmp":
                            ParseUtility.TryParseFloat(mapperArgValue, out uAmp);
                            break;

                        case "UFreq":
                            ParseUtility.TryParseFloat(mapperArgValue, out uFreq);
                            break;

                        case "UPhase":
                            ParseUtility.TryParseFloat(mapperArgValue, out uPhase);
                            break;

                        case "VAmp":
                            ParseUtility.TryParseFloat(mapperArgValue, out vAmp);
                            break;

                        case "VFreq":
                            ParseUtility.TryParseFloat(mapperArgValue, out vFreq);
                            break;

                        case "VPhase":
                            ParseUtility.TryParseFloat(mapperArgValue, out vPhase);
                            break;

                        case "BumpRotation":
                            ParseUtility.TryParseFloat(mapperArgValue, out bumpRotation);
                            break;

                        case "BumpScale":
                            ParseUtility.TryParseFloat(mapperArgValue, out bumpScale);
                            break;

                        case "UStep":
                            ParseUtility.TryParseFloat(mapperArgValue, out uStep);
                            break;

                        case "VStep":
                            ParseUtility.TryParseFloat(mapperArgValue, out vStep);
                            break;

                        case "SPS":
                            ParseUtility.TryParseFloat(mapperArgValue, out stepsPerSecond);
                            break;

                        case "Offset":
                            ParseUtility.TryParseFloat(mapperArgValue, out offset);
                            break;

                        case "Axis":
                            ParseUtility.TryParseFloat(mapperArgValue, out axis);
                            break;

                        case "UOffset":
                            ParseUtility.TryParseFloat(mapperArgValue, out uOffset);
                            break;

                        case "VOffset":
                            ParseUtility.TryParseFloat(mapperArgValue, out vOffset);
                            break;

                        case "ClampFix":
                            ParseUtility.TryParseFloat(mapperArgValue, out clampFix);
                            break;

                        case "UseReflect":
                            ParseUtility.TryParseFloat(mapperArgValue, out useReflect);
                            break;

                        case "VPreSec":
                            ParseUtility.TryParseFloat(mapperArgValue, out vPerSec);
                            break;

                        case "Period":
                            ParseUtility.TryParseFloat(mapperArgValue, out period);
                            break;

                        case "UperSec":
                            ParseUtility.TryParseFloat(mapperArgValue, out uPerSec);
                            break;

                        case "fps":
                            ParseUtility.TryParseFloat(mapperArgValue, out fps);
                            break;

                        case "VPerScale":
                            ParseUtility.TryParseFloat(mapperArgValue, out vPerScale);
                            break;

                        default:
                            throw new InvalidDataException($"Unknown mapper arg. Name = {mapperArgName}, Value = {splitMapperArg[1]}");
                    }
                }
            }

            return new W3dVertexMapperArgs(chunkType, value, uPerSec, vPerSec, uScale, vScale, fps, log1Width,
                log2Width, last, speed, uCenter, vCenter, uAmp, uFreq, uPhase, vAmp, vFreq, vPhase, uStep, vStep,
                stepsPerSecond, offset, axis, uOffset, vOffset, clampFix, useReflect, period, vPerScale, bumpRotation,
                bumpScale);
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
