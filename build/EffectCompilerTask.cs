using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace OpenSage.EffectCompiler
{
    public sealed class EffectCompilerTask : Task
    {
        [Required]
        public ITaskItem RootNamespace { get; set; }

        [Required]
        public ITaskItem ProjectDirectory { get; set; }

        [Required]
        public ITaskItem IntermediateDirectory { get; set; }

        [Required]
        public ITaskItem[] Files { get; set; }

        [Output]
        public ITaskItem[] CompileFiles { get; set; }

        public bool Debug { get; set; }

        public override bool Execute()
        {
            //System.Diagnostics.Debugger.Launch();

            var fxcPath = GetFxcPath();
            if (fxcPath == null)
            {
                return false;
            }

            var compileFiles = new List<ITaskItem>();

            foreach (var file in Files)
            {
                Log.LogMessage(MessageImportance.High, "Processing item {0}", file.ItemSpec);

                var compileFile = CompileShader(file, fxcPath);
                if (compileFile == null)
                {
                    return false;
                }

                compileFiles.Add(compileFile);
            }

            CompileFiles = compileFiles.ToArray();

            return true;
        }

        private ITaskItem CompileShader(ITaskItem item, string fxcPath)
        {
            var outputFolder = Path.Combine(
                ProjectDirectory.ItemSpec,
                IntermediateDirectory.ItemSpec,
                item.GetMetadata("RelativeDir"));

            var fileName = item.GetMetadata("Filename");

            var vertexShaderBytecode = CompileShader(item, fxcPath, "vs_5_0", "VS", "VERTEX_SHADER=1", outputFolder, fileName);
            var pixelShaderBytecode = CompileShader(item, fxcPath, "ps_5_0", "PS","PIXEL_SHADER=1",  outputFolder, fileName);

            if (vertexShaderBytecode == null || pixelShaderBytecode == null)
            {
                return null;
            }

            var fxoOutputPath = Path.Combine(
                outputFolder,
                fileName + ".fxo");

            using (var fxoStream = File.OpenWrite(fxoOutputPath))
            using (var binaryWriter = new BinaryWriter(fxoStream))
            {
                binaryWriter.Write(vertexShaderBytecode.Length);
                binaryWriter.Write(vertexShaderBytecode);

                binaryWriter.Write(pixelShaderBytecode.Length);
                binaryWriter.Write(pixelShaderBytecode);
            }

            var result = new TaskItem(fxoOutputPath);

            var embeddedResourceName = 
                RootNamespace.ItemSpec + "."
                + item.GetMetadata("RelativeDir").Replace('\\', '.')
                + fileName 
                + ".fxo";
            result.SetMetadata("EmbeddedResourceName", embeddedResourceName);

            return result;
        }

        private byte[] CompileShader(
            ITaskItem item, 
            string fxcPath, 
            string shaderProfile, 
            string shaderType, 
            string macro,
            string outputFolder,
            string fileName)
        {
            var fxoOutputPath = Path.Combine(outputFolder, fileName + shaderType + ".cso");

            return CompileShader(item, fxcPath, shaderProfile, shaderType, macro, fxoOutputPath);
        }

        private byte[] CompileShader(
            ITaskItem item, 
            string fxcPath, 
            string shaderProfile, 
            string entryPoint,
            string macro, 
            string outputPath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,                
                RedirectStandardOutput = true,
                FileName = fxcPath,
                Arguments = string.Format("{0} /nologo /T {1} /E {2} /D {3} /Fo {4}", item.ItemSpec, shaderProfile, entryPoint, macro, outputPath),
                CreateNoWindow = true
            };

            var standardOutput = string.Empty;
            var errorOutput = string.Empty;

            using (var process = new Process())
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    standardOutput += e.Data + "\n";
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    errorOutput += e.Data + "\n";
                };

                process.StartInfo = processStartInfo;
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }

            if (!string.IsNullOrEmpty(errorOutput))
            {
                if (!ParseAndLogErrors(item.ItemSpec, errorOutput))
                {
                    return null;
                }
            }

            if (!File.Exists(outputPath))
            {
                Log.LogMessage(MessageImportance.High, "Expected to find compiled shader at '{0}', but didn't.", outputPath);
                return null;
            }

            return File.ReadAllBytes(outputPath);
        }

        private static readonly Regex LocationRegex = new Regex(@"([\s\S]+)\((\d+),(\d+)-(\d+)\): ");
        private static readonly Regex ErrorRegex = new Regex(@"(\w+) (\w+): ([\s\S]+)");

        private bool ParseAndLogErrors(string inputFile, string errorOutput)
        {
            var anyErrors = false;

            foreach (var line in errorOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var locationMatch = LocationRegex.Match(line);

                string fileName;
                int lineNumber, columnStart, columnEnd;

                if (locationMatch.Success)
                {
                    fileName = locationMatch.Groups[1].Value;
                    lineNumber = Convert.ToInt32(locationMatch.Groups[2].Value);
                    columnStart = Convert.ToInt32(locationMatch.Groups[3].Value);
                    columnEnd = Convert.ToInt32(locationMatch.Groups[4].Value);
                }
                else
                {
                    fileName = inputFile;
                    lineNumber = columnStart = columnEnd = 0;
                }

                string messageType, messageCode, message;

                var errorMatch = ErrorRegex.Match(line);
                if (errorMatch.Success)
                {
                    messageType = errorMatch.Groups[1].Value;
                    messageCode = errorMatch.Groups[2].Value;
                    message = errorMatch.Groups[3].Value;
                }
                else
                {
                    messageType = "error";
                    messageCode = string.Empty;
                    message = line;
                }

                switch (messageType)
                {
                    case "warning":
                        Log.LogWarning(
                            string.Empty, 
                            messageCode, 
                            string.Empty,
                            fileName,
                            lineNumber,
                            columnStart,
                            lineNumber,
                            columnEnd,
                            message);
                        break;

                    case "error":
                    default:
                        anyErrors = true;
                        Log.LogError(
                            string.Empty, 
                            messageCode, 
                            string.Empty,
                            fileName,
                            lineNumber,
                            columnStart,
                            lineNumber,
                            columnEnd,
                            message);
                        break;
                }
            }

            return !anyErrors;
        }

        private string GetFxcPath()
        {
            string result = null;
            Version fxcVersion = null;

            foreach (var windowsKitBinaryPath in GetWindowsKitBinaryPaths())
            {
                var fxcPath = Path.Combine(windowsKitBinaryPath, "fxc.exe");
                if (!File.Exists(fxcPath))
                {
                    continue;
                }

                var fileVersion = FileVersionInfo.GetVersionInfo(fxcPath);
                var version = new Version(
                    fileVersion.FileMajorPart,
                    fileVersion.FileMinorPart,
                    fileVersion.FileBuildPart,
                    fileVersion.FilePrivatePart);

                if (fxcVersion == null || version > fxcVersion)
                {
                    result = fxcPath;
                    fxcVersion = version;
                }
            }

            if (result == null)
            {
                Log.LogMessage(MessageImportance.High, "Could not find path to fxc.exe");
            }

            return result;
        }

        private static IEnumerable<string> GetWindowsKitBinaryPaths()
        {
            var result = new List<string>();

            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows Kits\Installed Roots"))
            {
                var kitRootNames = key.GetValueNames()
                    .Where(x => !string.IsNullOrEmpty(x) && x.StartsWith("KitsRoot"));

                foreach (var kitRootName in kitRootNames)
                {
                    var kitRootPath = (string) key.GetValue(kitRootName);
                    if (kitRootPath == null)
                    {
                        continue;
                    }

                    // New SDK versions put a version number key under InstalledRoots.
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var versionSubKey = key.OpenSubKey(subKeyName))
                        {
                            var versionedBinaryPath = Path.Combine(kitRootPath, "bin", subKeyName, "x86");
                            if (Directory.Exists(versionedBinaryPath))
                            {
                                result.Add(versionedBinaryPath);
                            }
                        }
                    }

                    // Old SDK versions put the binaries directly under the kit root.
                    var binaryPath = Path.Combine(kitRootPath, "bin", "x86");
                    if (Directory.Exists(binaryPath))
                    {
                        result.Add(binaryPath);
                    }
                }
            }

            return result;
        }
    }
}
