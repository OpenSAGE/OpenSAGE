using System.Collections.Generic;
using Vortice.Dxc;

namespace OpenSage.Rendering.Effects.EffectCompiler;

internal static class DxcHelper
{
    public static DxcCompilationResult RunDxc(
        string hlslPath,
        string hlslCode,
        string entryPoint,
        string targetProfile,
        bool debug)
    {
        var arguments = new List<string>();

        arguments.Add(hlslPath);

        arguments.Add("-E");
        arguments.Add(entryPoint);

        arguments.Add("-T");
        arguments.Add(targetProfile);

        if (debug)
        {
            arguments.Add("-Zi");
        }

        arguments.Add("-WX");
        arguments.Add("-Wno-effects-syntax");

        arguments.Add("-spirv");

        using var dxcResult = DxcCompiler.Compile(
            hlslCode,
            arguments.ToArray(),
            null);

        var spirvBytes = dxcResult.GetObjectBytecodeArray();

        return new DxcCompilationResult(
            dxcResult.GetStatus().Success,
            spirvBytes,
            dxcResult.GetErrors());
    }
}

internal readonly record struct DxcCompilationResult(bool Successful, byte[] Spirv, string Messages);
