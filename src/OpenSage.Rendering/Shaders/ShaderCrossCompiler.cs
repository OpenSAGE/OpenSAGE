using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace OpenSage.Rendering;

internal static class ShaderCrossCompiler
{
    private const string EntryPoint = "main";

#if DEBUG
    private const bool UseDebugCompilation = true;
#else
    private const bool UseDebugCompilation = false;
#endif

    public static ShaderCacheFile GetOrCreateCachedShaders(
        ResourceFactory factory,
        Assembly shaderAssembly,
        string shaderName)
    {
        const string shaderCacheFolder = "ShaderCache";
        var backendType = factory.BackendType;
        var targetExtension = backendType.ToString().ToLowerInvariant();

        if (!Directory.Exists(shaderCacheFolder))
        {
            Directory.CreateDirectory(shaderCacheFolder);
        }

        var vsSpvBytes = ReadShaderSpv(shaderAssembly, shaderName, "vert");
        var fsSpvBytes = ReadShaderSpv(shaderAssembly, shaderName, "frag");

        var spvHash = GetShaderHash(vsSpvBytes, fsSpvBytes);

        // Look for cached shader file on disk match the input SPIR-V shaders.
        var cacheFilePath = Path.Combine(shaderCacheFolder, $"OpenSage.Assets.Shaders.{shaderName}.{spvHash}.{targetExtension}");

        if (ShaderCacheFile.TryLoad(cacheFilePath, out var shaderCacheFile))
        {
            // Cache is valid - use it.
            return shaderCacheFile;
        }

        // Cache is invalid or doesn't exist - do cross-compilation.

        // For Vulkan, we don't actually need to do cross-compilation. But we do need to get reflection data.
        // So we cross-compile to HLSL, throw away the resulting HLSL, and use the reflection data.
        var compilationTarget = backendType == GraphicsBackend.Vulkan
            ? CrossCompileTarget.HLSL
            : GetCompilationTarget(backendType);

        var compilationResult = SpirvCompilation.CompileVertexFragment(
            vsSpvBytes,
            fsSpvBytes,
            compilationTarget,
            new CrossCompileOptions());

        byte[] vsBytes, fsBytes;

        switch (backendType)
        {
            case GraphicsBackend.Vulkan:
                vsBytes = vsSpvBytes;
                fsBytes = fsSpvBytes;
                break;

            case GraphicsBackend.Direct3D11:
                vsBytes = CompileHlsl(compilationResult.VertexShader, "vs_5_0");
                fsBytes = CompileHlsl(compilationResult.FragmentShader, "ps_5_0");
                break;

            case GraphicsBackend.OpenGL:
            case GraphicsBackend.OpenGLES:
                vsBytes = Encoding.ASCII.GetBytes(compilationResult.VertexShader);
                fsBytes = Encoding.ASCII.GetBytes(compilationResult.FragmentShader);
                break;

            case GraphicsBackend.Metal:
                // TODO: Compile to IR.
                vsBytes = Encoding.UTF8.GetBytes(compilationResult.VertexShader);
                fsBytes = Encoding.UTF8.GetBytes(compilationResult.FragmentShader);
                break;

            default:
                throw new InvalidOperationException();
        }

        var entryPoint = factory.BackendType == GraphicsBackend.Metal
            ? $"{EntryPoint}0"
            : EntryPoint;

        shaderCacheFile = new ShaderCacheFile(
            new ShaderDescription(ShaderStages.Vertex, vsBytes, entryPoint),
            new ShaderDescription(ShaderStages.Fragment, fsBytes, entryPoint),
            compilationResult.Reflection.ResourceLayouts);

        shaderCacheFile.Save(cacheFilePath);

        return shaderCacheFile;
    }

    private static byte[] ReadShaderSpv(Assembly assembly, string shaderName, string shaderType)
    {
        var bytecodeShaderName = $"OpenSage.Assets.Shaders.{shaderName}.{shaderType}.spv";
        using (var shaderStream = assembly.GetManifestResourceStream(bytecodeShaderName))
        using (var memoryStream = new MemoryStream())
        {
            shaderStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

    private static byte[] CompileHlsl(string hlsl, string profile)
    {
        var flags = UseDebugCompilation
            ? Vortice.D3DCompiler.ShaderFlags.Debug
            : Vortice.D3DCompiler.ShaderFlags.OptimizationLevel3;

        var compilationResult = Vortice.D3DCompiler.Compiler.Compile(
            hlsl,
            EntryPoint,
            "HLSL",
            profile,
            out Vortice.Direct3D.Blob result,
            out Vortice.Direct3D.Blob error);

        if (compilationResult.Failure)
        {
            throw new Exception(error.ToString());
        }

        return result.GetBytes();
    }

    private static string GetShaderHash(byte[] vsBytes, byte[] fsBytes)
    {
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        sha256.AppendData(vsBytes);
        sha256.AppendData(fsBytes);

        var hash = sha256.GetCurrentHash();

        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString();
    }

    private static CrossCompileTarget GetCompilationTarget(GraphicsBackend backend)
    {
        return backend switch
        {
            GraphicsBackend.Direct3D11 => CrossCompileTarget.HLSL,
            GraphicsBackend.OpenGL => CrossCompileTarget.GLSL,
            GraphicsBackend.Metal => CrossCompileTarget.MSL,
            GraphicsBackend.OpenGLES => CrossCompileTarget.ESSL,
            _ => throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}"),
        };
    }
}
