using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using Veldrid;

namespace OpenSage.Rendering.Effects.EffectCompiler;

public static class EffectCompiler
{
    public static void CompileToFile(string effectPath, GraphicsBackend targetBackend, string outputPath)
    {
        var result = Compile(effectPath, targetBackend);

        if (!result.Successful)
        {
            foreach (var message in result.Messages)
            {
                throw new InvalidOperationException(message);
            }
        }

        using var effectStream = File.OpenWrite(outputPath);
        using var effectWriter = new BinaryWriter(effectStream);

        result.EffectContent.WriteTo(effectWriter);
    }

    internal static EffectCompilationResult Compile(string effectPath, GraphicsBackend targetBackend)
    {
        var text = File.ReadAllText(effectPath);

        var sourceFile = new SourceFile(SourceText.From(text), effectPath);
        var compilationUnit = SyntaxFactory.ParseCompilationUnit(sourceFile);

        if (compilationUnit.ContainsDiagnostics)
        {
            var messages = new List<string>();

            foreach (var diagnostic in compilationUnit.GetDiagnostics())
            {
                var fileSpan = compilationUnit.SyntaxTree.GetSourceFileSpan(diagnostic.SourceRange);
                var linePosition = fileSpan.File.Text.Lines.GetLinePosition(fileSpan.Span.Start);

                var location = $"{fileSpan.File.FilePath}({linePosition.Line + 1},{linePosition.Character + 1})";
                var severity = diagnostic.Descriptor.Severity.ToString().ToLowerInvariant();
                var diagnosticId = diagnostic.Descriptor.Id;

                messages.Add($"{location}: {severity} {diagnosticId}: {diagnostic.Message}");
            }

            return new EffectCompilationResult(false, null, messages);
        }

        var effectContent = new EffectContent();

        foreach (var declaration in compilationUnit.Declarations)
        {
            if (declaration.Kind != SyntaxKind.TechniqueDeclaration)
            {
                continue;
            }

            var techniqueSyntax = (TechniqueSyntax)declaration;

            var techniqueContent = new EffectTechniqueContent
            {
                Name = techniqueSyntax.Name.ValueText
            };
            effectContent.Techniques.Add(techniqueContent);

            foreach (var pass in techniqueSyntax.Passes)
            {
                var passContent = new EffectPassContent
                {
                    Name = pass.Name.ValueText
                };
                techniqueContent.Passes.Add(passContent);

                // TODO: Call dxc.exe for each vertex and pixel shader.
                // Save output SPIR-V into custom binary.
                // Along with reflection data.

                byte[] vsBytes = null;
                byte[] fsBytes = null;

                string vsEntryPoint = null;
                string fsEntryPoint = null;

                foreach (var passStatementSyntax in pass.Statements)
                {
                    switch (passStatementSyntax)
                    {
                        case ExpressionStatementSyntax expressionStatement:
                            switch (expressionStatement.Expression)
                            {
                                case AssignmentExpressionSyntax assignmentExpression:
                                    switch (assignmentExpression.Left)
                                    {
                                        case IdentifierNameSyntax identifierName:
                                            switch (identifierName.Name.ValueText)
                                            {
                                                case "VertexShader":
                                                    if (!HandleShaderProperty(effectPath, text, assignmentExpression.Right, out vsBytes, out vsEntryPoint, out var vsMessages))
                                                    {
                                                        return new EffectCompilationResult(false, null, new[] { vsMessages });
                                                    }
                                                    break;

                                                case "PixelShader":
                                                    if (!HandleShaderProperty(effectPath, text, assignmentExpression.Right, out fsBytes, out fsEntryPoint, out var fsMessages))
                                                    {
                                                        return new EffectCompilationResult(false, null, new[] { fsMessages });
                                                    }
                                                    break;

                                                case "SrcBlend":
                                                    passContent.BlendStateDescription.AttachmentStates[0].SourceColorFactor = GetBlendFactor(assignmentExpression.Right);
                                                    break;

                                                case "DestBlend":
                                                    passContent.BlendStateDescription.AttachmentStates[0].DestinationColorFactor = GetBlendFactor(assignmentExpression.Right);
                                                    break;

                                                case "BlendOp":
                                                    passContent.BlendStateDescription.AttachmentStates[0].ColorFunction = GetBlendFunction(assignmentExpression.Right);
                                                    break;

                                                case "SrcBlendAlpha":
                                                    passContent.BlendStateDescription.AttachmentStates[0].SourceAlphaFactor = GetBlendFactor(assignmentExpression.Right);
                                                    break;

                                                case "DestBlendAlpha":
                                                    passContent.BlendStateDescription.AttachmentStates[0].DestinationAlphaFactor = GetBlendFactor(assignmentExpression.Right);
                                                    break;

                                                case "BlendOpAlpha":
                                                    passContent.BlendStateDescription.AttachmentStates[0].AlphaFunction = GetBlendFunction(assignmentExpression.Right);
                                                    break;

                                                case "ZEnable":
                                                    passContent.DepthStencilStateDescription.DepthTestEnabled = GetBoolean(assignmentExpression.Right);
                                                    break;

                                                case "ZWriteEnable":
                                                    passContent.DepthStencilStateDescription.DepthWriteEnabled = GetBoolean(assignmentExpression.Right);
                                                    break;

                                                case "ZFunc":
                                                    passContent.DepthStencilStateDescription.DepthComparison = GetComparisonKind(assignmentExpression.Right);
                                                    break;

                                                case "CullMode":
                                                    passContent.RasterizerStateDescription.CullMode = GetCullMode(assignmentExpression.Right);
                                                    break;

                                                case "FillMode":
                                                    passContent.RasterizerStateDescription.FillMode = GetFillMode(assignmentExpression.Right);
                                                    break;

                                                case "FrontCounterClockwise":
                                                    passContent.RasterizerStateDescription.FrontFace = GetBoolean(assignmentExpression.Right) ? FrontFace.CounterClockwise : FrontFace.Clockwise;
                                                    break;

                                                case "DepthClipEnable":
                                                    passContent.RasterizerStateDescription.DepthClipEnabled = GetBoolean(assignmentExpression.Right);
                                                    break;

                                                case "ScissorEnable":
                                                    passContent.RasterizerStateDescription.ScissorTestEnabled = GetBoolean(assignmentExpression.Right);
                                                    break;

                                                default:
                                                    throw new InvalidOperationException();
                                            }
                                            break;

                                        case ElementAccessExpressionSyntax elementAccessExpression:
                                            var index = elementAccessExpression.Index switch
                                            {
                                                LiteralExpressionSyntax literalExpression => (int)literalExpression.Token.Value,
                                                _ => throw new InvalidOperationException()
                                            };

                                            switch (elementAccessExpression.Expression)
                                            {
                                                case IdentifierNameSyntax identifierName:
                                                    switch (identifierName.Name.ValueText)
                                                    {
                                                        case "BlendEnable":
                                                            passContent.BlendStateDescription.AttachmentStates[index].BlendEnabled = GetBoolean(assignmentExpression.Right);
                                                            break;

                                                        default:
                                                            throw new InvalidOperationException();
                                                    }
                                                    break;

                                                default:
                                                    throw new InvalidOperationException();
                                            }
                                            break;

                                        default:
                                            throw new InvalidOperationException();
                                    }
                                    break;
                            }
                            break;
                    }
                }

                if (vsBytes == null || fsBytes == null)
                {
                    throw new InvalidOperationException();
                }

                // Cross-compile SPIR-V to MetalSL. We'll use this for reflection data
                // that's applicable to all shading languages.
                var spirvCompilationResult = SpirvCrossHelper.RunSpirvCross(
                    vsEntryPoint, vsBytes,
                    fsEntryPoint, fsBytes,
                    GraphicsBackend.Metal);

                passContent.VertexElements = spirvCompilationResult.VertexElements;
                passContent.ResourceLayouts = spirvCompilationResult.ResourceLayouts;

                switch (targetBackend)
                {
                    case GraphicsBackend.Direct3D11:
                        CompileDirect3D11Shaders(
                            vsEntryPoint, vsBytes,
                            fsEntryPoint, fsBytes,
                            out var vsBytesHlsl, out var fsBytesHlsl);
                        passContent.ShaderSets.Add(new EffectShaderSetContent
                        {
                            Backend = GraphicsBackend.Direct3D11,
                            Shaders =
                            {
                                new ShaderDescription(ShaderStages.Vertex, vsBytesHlsl, "main"),
                                new ShaderDescription(ShaderStages.Fragment, fsBytesHlsl, "main"),
                            }
                        });
                        break;

                    case GraphicsBackend.Vulkan:
                        // For Vulkan, use the SPIR-V we've just compiled.
                        passContent.ShaderSets.Add(new EffectShaderSetContent
                        {
                            Backend = GraphicsBackend.Vulkan,
                            Shaders =
                            {
                                new ShaderDescription(ShaderStages.Vertex, vsBytes, "main"),
                                new ShaderDescription(ShaderStages.Fragment, fsBytes, "main"),
                            }
                        });
                        break;

                    case GraphicsBackend.Metal:
                        // For Metal, use the MetalSL code we've already compiled with SPIRV-Cross.
                        passContent.ShaderSets.Add(new EffectShaderSetContent
                        {
                            Backend = GraphicsBackend.Metal,
                            Shaders =
                            {
                                new ShaderDescription(ShaderStages.Vertex, spirvCompilationResult.VsCode, "main0"),
                                new ShaderDescription(ShaderStages.Fragment, spirvCompilationResult.FsCode, "main0"),
                            }
                        });
                        break;

                    case GraphicsBackend.OpenGL:
                        // Cross-compile to GLSL.
                        var spirvCompilationResultGlsl = SpirvCrossHelper.RunSpirvCross(
                            vsEntryPoint, vsBytes,
                            fsEntryPoint, fsBytes,
                            GraphicsBackend.OpenGL);
                        passContent.ShaderSets.Add(new EffectShaderSetContent
                        {
                            Backend = GraphicsBackend.OpenGL,
                            Shaders =
                            {
                                new ShaderDescription(ShaderStages.Vertex, spirvCompilationResultGlsl.VsCode, "main"),
                                new ShaderDescription(ShaderStages.Fragment, spirvCompilationResultGlsl.FsCode, "main"),
                            }
                        });
                        break;
                }
            }
        }

        return new EffectCompilationResult(true, effectContent, Array.Empty<string>());
    }

    private static BlendFactor GetBlendFactor(ExpressionSyntax expression)
    {
        if (expression is not IdentifierNameSyntax identifierName)
        {
            throw new InvalidOperationException();
        }

        return identifierName.Name.ValueText switch
        {
            "ONE" => BlendFactor.One,
            "SRC_ALPHA" => BlendFactor.SourceAlpha,
            "INV_SRC_ALPHA" => BlendFactor.InverseSourceAlpha,
            _ => throw new InvalidOperationException()
        };
    }

    private static BlendFunction GetBlendFunction(ExpressionSyntax expression)
    {
        if (expression is not IdentifierNameSyntax identifierName)
        {
            throw new InvalidOperationException();
        }

        return identifierName.Name.ValueText switch
        {
            "ADD" => BlendFunction.Add,
            _ => throw new InvalidOperationException()
        };
    }

    private static ComparisonKind GetComparisonKind(ExpressionSyntax expression)
    {
        if (expression is not IdentifierNameSyntax identifierName)
        {
            throw new InvalidOperationException();
        }

        return identifierName.Name.ValueText switch
        {
            "LESSEQUAL" => ComparisonKind.LessEqual,
            _ => throw new InvalidOperationException()
        };
    }

    private static FaceCullMode GetCullMode(ExpressionSyntax expression)
    {
        if (expression is not IdentifierNameSyntax identifierName)
        {
            throw new InvalidOperationException();
        }

        return identifierName.Name.ValueText switch
        {
            "BACK" => FaceCullMode.Back,
            _ => throw new InvalidOperationException()
        };
    }

    private static PolygonFillMode GetFillMode(ExpressionSyntax expression)
    {
        if (expression is not IdentifierNameSyntax identifierName)
        {
            throw new InvalidOperationException();
        }

        return identifierName.Name.ValueText switch
        {
            "SOLID" => PolygonFillMode.Solid,
            _ => throw new InvalidOperationException()
        };
    }

    private static bool GetBoolean(ExpressionSyntax expression)
    {
        if (expression is not LiteralExpressionSyntax literalExpression)
        {
            throw new InvalidOperationException();
        }

        return literalExpression.Kind switch
        {
            SyntaxKind.TrueLiteralExpression => true,
            SyntaxKind.FalseLiteralExpression => false,
            _ => throw new InvalidOperationException(),
        };
    }

    private static bool HandleShaderProperty(
        string effectPath,
        string effectCode,
        ExpressionSyntax expression,
        out byte[] spirvBytes,
        out string entryPoint,
        out string messages)
    {
        if (!(expression is CompileExpressionSyntax compileExpression))
        {
            throw new InvalidOperationException();
        }

        var targetProfile = compileExpression.ShaderTargetToken.ValueText;
        entryPoint = ((IdentifierNameSyntax)compileExpression.ShaderFunction.Name).Name.ValueText;

        // Compile HLSL to SPIR-V using dxc.
        var result = DxcHelper.RunDxc(
            effectPath,
            effectCode,
            entryPoint,
            targetProfile,
            true);

        spirvBytes = result.Spirv;
        messages = result.Messages;

        return result.Successful;
    }

    private static void CompileDirect3D11Shaders(
        string vsEntryPoint, byte[] vsBytes,
        string fsEntryPoint, byte[] fsBytes,
        out byte[] vsBytesHlsl, out byte[] fsBytesHlsl)
    {
        var spirvCompilationResult = SpirvCrossHelper.RunSpirvCross(
            vsEntryPoint, vsBytes,
            fsEntryPoint, fsBytes,
            GraphicsBackend.Direct3D11);

        vsBytesHlsl = spirvCompilationResult.VsCode;
        fsBytesHlsl = spirvCompilationResult.FsCode;
    }
}

internal readonly record struct EffectCompilationResult(bool Successful, EffectContent EffectContent, IEnumerable<string> Messages);
