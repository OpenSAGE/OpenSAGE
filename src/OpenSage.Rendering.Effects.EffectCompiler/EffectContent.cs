using System.Collections.Generic;
using System.IO;
using Veldrid;

namespace OpenSage.Rendering.Effects.EffectCompiler;

internal sealed class EffectContent
{
    public readonly List<EffectTechniqueContent> Techniques = new();

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(1); // Version
        writer.Write(Techniques.Count);
        foreach (var technique in Techniques)
        {
            technique.WriteTo(writer);
        }
    }
}

internal sealed class EffectTechniqueContent
{
    public string Name;
    public readonly List<EffectPassContent> Passes = new();

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Name);

        writer.Write(Passes.Count);
        foreach (var pass in Passes)
        {
            pass.WriteTo(writer);
        }
    }
}

internal sealed class EffectPassContent
{
    public string Name;
    public readonly List<EffectShaderSetContent> ShaderSets = new();
    public VertexElementDescription[] VertexElements;
    public ResourceLayoutDescription[] ResourceLayouts;
    public BlendStateDescription BlendStateDescription;
    public DepthStencilStateDescription DepthStencilStateDescription = DepthStencilStateDescription.DepthOnlyLessEqual;
    public RasterizerStateDescription RasterizerStateDescription = RasterizerStateDescription.Default;

    public EffectPassContent()
    {
        BlendStateDescription = new(RgbaFloat.White)
        {
            AttachmentStates = new BlendAttachmentDescription[8]
        };
        for (var i = 0; i < BlendStateDescription.AttachmentStates.Length; i++)
        {
            BlendStateDescription.AttachmentStates[i] = BlendAttachmentDescription.Disabled;
        }
    }

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Name);

        writer.Write(ShaderSets.Count);
        foreach (var shaderSet in ShaderSets)
        {
            shaderSet.WriteTo(writer);
        }

        writer.Write(VertexElements.Length);
        foreach (var vertexElement in VertexElements)
        {
            writer.Write(vertexElement.Name);
            writer.Write((byte)vertexElement.Semantic);
            writer.Write((byte)vertexElement.Format);
            writer.Write(vertexElement.Offset);
        }

        writer.Write(ResourceLayouts.Length);
        foreach (var resourceLayout in ResourceLayouts)
        {
            writer.Write(resourceLayout.Elements.Length);
            foreach (var resourceLayoutElement in resourceLayout.Elements)
            {
                writer.Write(resourceLayoutElement.Name);
                writer.Write((byte)resourceLayoutElement.Kind);
                writer.Write((byte)resourceLayoutElement.Stages);
                writer.Write((int)resourceLayoutElement.Options);
            }
        }

        writer.Write(BlendStateDescription.BlendFactor.R);
        writer.Write(BlendStateDescription.BlendFactor.G);
        writer.Write(BlendStateDescription.BlendFactor.B);
        writer.Write(BlendStateDescription.BlendFactor.A);

        writer.Write(BlendStateDescription.AttachmentStates.Length);
        foreach (var attachmentState in BlendStateDescription.AttachmentStates)
        {
            writer.Write(attachmentState.BlendEnabled);
            writer.Write((byte)attachmentState.SourceColorFactor);
            writer.Write((byte)attachmentState.DestinationColorFactor);
            writer.Write((byte)attachmentState.ColorFunction);
            writer.Write((byte)attachmentState.SourceAlphaFactor);
            writer.Write((byte)attachmentState.DestinationAlphaFactor);
            writer.Write((byte)attachmentState.AlphaFunction);
        }

        writer.Write(DepthStencilStateDescription.DepthTestEnabled);
        writer.Write(DepthStencilStateDescription.DepthWriteEnabled);
        writer.Write((byte)DepthStencilStateDescription.DepthComparison);
        writer.Write(DepthStencilStateDescription.StencilTestEnabled);
        writer.Write(DepthStencilStateDescription.StencilFront);
        writer.Write(DepthStencilStateDescription.StencilBack);
        writer.Write(DepthStencilStateDescription.StencilReadMask);
        writer.Write(DepthStencilStateDescription.StencilWriteMask);
        writer.Write(DepthStencilStateDescription.StencilReference);

        writer.Write((byte)RasterizerStateDescription.CullMode);
        writer.Write((byte)RasterizerStateDescription.FillMode);
        writer.Write((byte)RasterizerStateDescription.FrontFace);
        writer.Write(RasterizerStateDescription.DepthClipEnabled);
        writer.Write(RasterizerStateDescription.ScissorTestEnabled);
    }
}

internal static class BinaryWriterExtensions
{
    public static void Write(this BinaryWriter writer, in StencilBehaviorDescription value)
    {
        writer.Write((byte)value.Fail);
        writer.Write((byte)value.Pass);
        writer.Write((byte)value.DepthFail);
        writer.Write((byte)value.Comparison);
    }
}

internal sealed class EffectShaderSetContent
{
    public GraphicsBackend Backend;
    public readonly List<ShaderDescription> Shaders = new();

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write((byte)Backend);

        // Write placeholder length; we'll fix this up with the actual length.
        var lengthPosition = writer.BaseStream.Position;
        writer.Write(0u);

        var startPosition = writer.BaseStream.Position;

        writer.Write(Shaders.Count);
        foreach (var shader in Shaders)
        {
            writer.Write((byte)shader.Stage);

            writer.Write(shader.ShaderBytes.Length);
            writer.Write(shader.ShaderBytes);

            writer.Write(shader.EntryPoint);

            writer.Write(shader.Debug);
        }

        var endPosition = writer.BaseStream.Position;
        var length = (uint)(endPosition - startPosition);

        writer.BaseStream.Seek(lengthPosition, SeekOrigin.Begin);
        writer.Write(length);

        writer.BaseStream.Seek(endPosition, SeekOrigin.Begin);
    }
}
