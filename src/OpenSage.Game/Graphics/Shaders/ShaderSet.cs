using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OpenSage.Utilities.Extensions;
using Veldrid;
using Veldrid.SPIRV;

namespace OpenSage.Graphics.Shaders
{
    public sealed class ShaderSet : DisposableBase
    {
        private const string EntryPoint = "main";

        private static byte NextId;

        public readonly byte Id;

        public readonly ShaderSetDescription Description;
        public readonly GlobalResourceSetIndices GlobalResourceSetIndices;

        public ShaderSet(
            GraphicsDevice graphicsDevice,
            string shaderName,
            GlobalResourceSetIndices globalResourceSetIndices,
            params VertexLayoutDescription[] vertexDescriptors)
        {
            GlobalResourceSetIndices = globalResourceSetIndices;

            Id = NextId++;

#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif

            GetShaders(
                graphicsDevice.ResourceFactory,
                shaderName,
                debug,
                out var vertexShader,
                out var fragmentShader);

            AddDisposable(vertexShader);
            AddDisposable(fragmentShader);

            vertexShader.Name = $"{shaderName}.vert";
            fragmentShader.Name = $"{shaderName}.frag";

            Description = new ShaderSetDescription(
                vertexDescriptors,
                new[] { vertexShader, fragmentShader });
        }

        private static void GetShaders(
            ResourceFactory factory,
            string shaderName,
            bool debug,
            out Shader vertexShader,
            out Shader fragmentShader)
        {
            GetOrCreateCachedShaders(
                factory,
                shaderName,
                debug,
                out var vsBytes,
                out var fsBytes);

            var entryPoint = factory.BackendType == GraphicsBackend.Metal
                ? $"{EntryPoint}0"
                : EntryPoint;

            vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vsBytes, entryPoint));
            fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fsBytes, entryPoint));
        }

        private static void GetOrCreateCachedShaders(
            ResourceFactory factory,
            string shaderName,
            bool debug,
            out byte[] vsBytes,
            out byte[] fsBytes)
        {
            const string shaderCacheFolder = "ShaderCache";
            var backendType = factory.BackendType;
            var targetExtension = backendType.ToString().ToLowerInvariant();

            if (!Directory.Exists(shaderCacheFolder))
            {
                Directory.CreateDirectory(shaderCacheFolder);
            }

            var vsSpvName = $"OpenSage.Assets.Shaders.{shaderName}.vert.spv";
            var fsSpvName = $"OpenSage.Assets.Shaders.{shaderName}.frag.spv";

            var vsSpvBytes = ReadSpvShader(vsSpvName);
            var fsSpvBytes = ReadSpvShader(fsSpvName);

            var vsSpvHash = GetShaderHash(vsSpvBytes);
            var fsSpvHash = GetShaderHash(fsSpvBytes);

            // Check that SPIR-V files on disk match what's in the assembly.
            var vsCacheFilePath = Path.Combine(shaderCacheFolder, $"OpenSage.Assets.Shaders.{shaderName}.vert.{vsSpvHash}.{targetExtension}");
            var fsCacheFilePath = Path.Combine(shaderCacheFolder, $"OpenSage.Assets.Shaders.{shaderName}.frag.{fsSpvHash}.{targetExtension}");

            if (File.Exists(vsCacheFilePath) &&
                File.Exists(fsCacheFilePath))
            {
                // Cache is valid - use it.
                vsBytes = File.ReadAllBytes(vsCacheFilePath);
                fsBytes = File.ReadAllBytes(fsCacheFilePath);
                return;
            }

            // Cache is invalid or doesn't exist - do cross-compilation.

            if (backendType == GraphicsBackend.Vulkan)
            {
                File.WriteAllBytes(vsCacheFilePath, vsSpvBytes);
                File.WriteAllBytes(fsCacheFilePath, fsSpvBytes);
                vsBytes = vsSpvBytes;
                fsBytes = fsSpvBytes;
                return;
            }

            var compilationTarget = GetCompilationTarget(backendType);
            var compilationResult = SpirvCompilation.CompileVertexFragment(
                vsSpvBytes,
                fsSpvBytes,
                compilationTarget,
                new CrossCompileOptions());

            switch (backendType)
            {
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

            File.WriteAllBytes(vsCacheFilePath, vsBytes);
            File.WriteAllBytes(fsCacheFilePath, fsBytes);
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

        private static byte[] ReadSpvShader(string spvShaderName)
        {
            var assembly = typeof(ShaderSet).Assembly;
            using var shaderStream = assembly.GetManifestResourceStream(spvShaderName);
            return shaderStream.ReadAllBytes();
        }

        private static string GetShaderHash(byte[] shaderBytes)
        {
            var hash = SHA1.HashData(shaderBytes);

            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        private static CrossCompileTarget GetCompilationTarget(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11:
                    return CrossCompileTarget.HLSL;

                case GraphicsBackend.OpenGL:
                    return CrossCompileTarget.GLSL;

                case GraphicsBackend.Metal:
                    return CrossCompileTarget.MSL;

                case GraphicsBackend.OpenGLES:
                    return CrossCompileTarget.ESSL;

                default:
                    throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}");
            }
        }
    }

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
}
