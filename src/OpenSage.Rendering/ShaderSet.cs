using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace OpenSage.Rendering;

public sealed class ShaderSet : DisposableBase
{
    private const string EntryPoint = "main";

    // TODO: Get this ID from elsewhere, not a static field.
    private static byte NextId;

    public readonly byte Id;
    public readonly ShaderSetDescription Description;
    public readonly ResourceLayout[] ResourceLayouts;

    // TODO: Remove this.
    public readonly GlobalResourceSetIndices GlobalResourceSetIndices;

    public ShaderSet(
        ResourceFactory factory,
        string shaderName,
        GlobalResourceSetIndices globalResourceSetIndices,
        VertexLayoutDescription[] vertexDescriptors)
    {
        Id = NextId++;

        GlobalResourceSetIndices = globalResourceSetIndices;

#if DEBUG
        const bool debug = true;
#else
        const bool debug = false;
#endif

        var cacheFile = GetOrCreateCachedShaders(factory, shaderName, debug);

        var vertexShader = AddDisposable(factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, cacheFile.VsBytes, cacheFile.VsEntryPoint)));
        vertexShader.Name = $"{shaderName}.vert";

        var fragmentShader = AddDisposable(factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, cacheFile.FsBytes, cacheFile.FsEntryPoint)));
        fragmentShader.Name = $"{shaderName}.frag";

        Description = new ShaderSetDescription(
            vertexDescriptors,
            new[] { vertexShader, fragmentShader });

        ResourceLayouts = new ResourceLayout[cacheFile.ResourceLayoutDescriptions.Length];
        for (var i = 0; i < cacheFile.ResourceLayoutDescriptions.Length; i++)
        {
            ResourceLayouts[i] = AddDisposable(factory.CreateResourceLayout(ref cacheFile.ResourceLayoutDescriptions[i]));
        }
    }

    private static ShaderCacheFile GetOrCreateCachedShaders(
        ResourceFactory factory,
        string shaderName,
        bool debug)
    {
        const string shaderCacheFolder = "ShaderCache";
        var backendType = factory.BackendType;
        var targetExtension = backendType.ToString().ToLowerInvariant();

        if (!Directory.Exists(shaderCacheFolder))
        {
            Directory.CreateDirectory(shaderCacheFolder);
        }

        var vsSpvName = $"Assets/Shaders/{shaderName}.vert.spv";
        var fsSpvName = $"Assets/Shaders/{shaderName}.frag.spv";

        var vsSpvBytes = File.ReadAllBytes(vsSpvName);
        var fsSpvBytes = File.ReadAllBytes(fsSpvName);

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
                vsBytes = CompileHlsl(compilationResult.VertexShader, "vs_5_0", debug);
                fsBytes = CompileHlsl(compilationResult.FragmentShader, "ps_5_0", debug);
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
            entryPoint,
            vsBytes,
            entryPoint,
            fsBytes,
            compilationResult.Reflection.ResourceLayouts);

        shaderCacheFile.Save(cacheFilePath);

        return shaderCacheFile;
    }

    private static byte[] CompileHlsl(string hlsl, string profile, bool debug)
    {
        var flags = debug
            ? SharpDX.D3DCompiler.ShaderFlags.Debug
            : SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel3;
        var compilationResult = SharpDX.D3DCompiler.ShaderBytecode.Compile(
            hlsl,
            EntryPoint,
            profile,
            flags);

        if (compilationResult.HasErrors)
        {
            throw new Exception(compilationResult.Message);
        }

        return compilationResult.Bytecode.Data;
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

    private sealed class ShaderCacheFile
    {
        private const int Version = 1;

        public readonly string VsEntryPoint;
        public readonly byte[] VsBytes;

        public readonly string FsEntryPoint;
        public readonly byte[] FsBytes;

        public readonly ResourceLayoutDescription[] ResourceLayoutDescriptions;

        public static bool TryLoad(string filePath, out ShaderCacheFile result)
        {
            if (!File.Exists(filePath))
            {
                result = null;
                return false;
            }

            using var fileStream = File.OpenRead(filePath);
            using var binaryReader = new BinaryReader(fileStream);

            var version = binaryReader.ReadInt32();
            if (version != Version)
            {
                result = null;
                return false;
            }

            var vsEntryPoint = binaryReader.ReadString();

            var vsBytesLength = binaryReader.ReadInt32();
            var vsBytes = new byte[vsBytesLength];
            for (var i = 0; i < vsBytesLength; i++)
            {
                vsBytes[i] = binaryReader.ReadByte();
            }

            var fsEntryPoint = binaryReader.ReadString();

            var fsBytesLength = binaryReader.ReadInt32();
            var fsBytes = new byte[fsBytesLength];
            for (var i = 0; i < fsBytesLength; i++)
            {
                fsBytes[i] = binaryReader.ReadByte();
            }

            var numResourceLayoutDescriptions = binaryReader.ReadInt32();
            var resourceLayoutDescriptions = new ResourceLayoutDescription[numResourceLayoutDescriptions];
            for (var i = 0; i < numResourceLayoutDescriptions; i++)
            {
                var numElements = binaryReader.ReadInt32();

                resourceLayoutDescriptions[i] = new ResourceLayoutDescription
                {
                    Elements = new ResourceLayoutElementDescription[numElements]
                };

                for (var j = 0; j < numElements; j++)
                {
                    resourceLayoutDescriptions[i].Elements[j] = new ResourceLayoutElementDescription
                    {
                        Name = binaryReader.ReadString(),
                        Kind = (ResourceKind)binaryReader.ReadByte(),
                        Stages = (ShaderStages)binaryReader.ReadByte(),
                        Options = (ResourceLayoutElementOptions)binaryReader.ReadByte(),
                    };
                }
            }

            result = new ShaderCacheFile(
                vsEntryPoint,
                vsBytes,
                fsEntryPoint,
                fsBytes,
                resourceLayoutDescriptions);

            return true;
        }

        public ShaderCacheFile(
            string vsEntryPoint,
            byte[] vsBytes,
            string fsEntryPoint,
            byte[] fsBytes,
            ResourceLayoutDescription[] resourceLayoutDescriptions)
        {
            VsEntryPoint = vsEntryPoint;
            VsBytes = vsBytes;

            FsEntryPoint = fsEntryPoint;
            FsBytes = fsBytes;

            ResourceLayoutDescriptions = resourceLayoutDescriptions;
        }

        public void Save(string filePath)
        {
            using var fileStream = File.OpenWrite(filePath);
            using var binaryWriter = new BinaryWriter(fileStream);

            binaryWriter.Write(Version);

            binaryWriter.Write(VsEntryPoint);

            binaryWriter.Write(VsBytes.Length);
            foreach (var value in VsBytes)
            {
                binaryWriter.Write(value);
            }

            binaryWriter.Write(FsEntryPoint);

            binaryWriter.Write(FsBytes.Length);
            foreach (var value in FsBytes)
            {
                binaryWriter.Write(value);
            }

            binaryWriter.Write(ResourceLayoutDescriptions.Length);
            for (var i = 0; i < ResourceLayoutDescriptions.Length; i++)
            {
                ref readonly var description = ref ResourceLayoutDescriptions[i];

                binaryWriter.Write(description.Elements.Length);

                for (var j = 0; j < description.Elements.Length; j++)
                {
                    ref readonly var element = ref description.Elements[j];

                    binaryWriter.Write(element.Name);
                    binaryWriter.Write((byte)element.Kind);
                    binaryWriter.Write((byte)element.Stages);
                    binaryWriter.Write((byte)element.Options);
                }
            }
        }
    }
}

// TODO: Remove this.
public sealed class GlobalResourceSetIndices
{
    public readonly uint? GlobalConstants;
    public readonly LightingType LightingType;
    public readonly uint? GlobalLightingConstants;
    public readonly uint? CloudConstants;
    public readonly uint? ShadowConstants;
    public readonly uint? RenderItemConstants;

    public GlobalResourceSetIndices(
        uint? globalConstants,
        LightingType lightingType,
        uint? globalLightingConstants,
        uint? cloudConstants,
        uint? shadowConstants,
        uint? renderItemConstants)
    {
        GlobalConstants = globalConstants;
        LightingType = lightingType;
        GlobalLightingConstants = globalLightingConstants;
        CloudConstants = cloudConstants;
        ShadowConstants = shadowConstants;
        RenderItemConstants = renderItemConstants;
    }
}

// TODO: Remove this from here.
public enum LightingType
{
    None,
    Terrain,
    Object
}
