using System;
using System.IO;
using Veldrid;

namespace OpenSage.Rendering.Effects;

public sealed class Effect : IDisposable
{
    public readonly EffectTechnique[] Techniques;

    public Effect(string filePath, GraphicsDevice graphicsDevice)
    {
        using (var stream = File.OpenRead(filePath))
        using (var reader = new BinaryReader(stream))
        {
            var version = reader.ReadInt32();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            var techniquesCount = reader.ReadInt32();
            Techniques = new EffectTechnique[techniquesCount];
            for (var i = 0; i < techniquesCount; i++)
            {
                Techniques[i] = new EffectTechnique(graphicsDevice, reader);
            }
        }
    }

    public EffectTechnique GetTechniqueByName(string name)
    {
        foreach (var technique in Techniques)
        {
            if (technique.Name == name)
            {
                return technique;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(name));
    }

    public void Dispose()
    {
        foreach (var technique in Techniques)
        {
            technique.Dispose();
        }
    }
}

public sealed class EffectTechnique : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;

    private OutputDescription _pipelineOutDescription;
    private Pipeline _pipeline;

    public readonly string Name;

    public readonly ShaderSetDescription ShaderSetDescription;

    public readonly ResourceLayout[] ResourceLayouts;

    public readonly BlendStateDescription BlendStateDescription;
    public readonly DepthStencilStateDescription DepthStencilStateDescription;
    public readonly RasterizerStateDescription RasterizerStateDescription;

    internal EffectTechnique(GraphicsDevice graphicsDevice, BinaryReader reader)
    {
        _graphicsDevice = graphicsDevice;

        Name = reader.ReadString();

        var passesCount = reader.ReadInt32();
        if (passesCount != 1)
        {
            throw new NotSupportedException();
        }

        var _ = reader.ReadString();

        ShaderDescription[] shaderDescriptions = null;

        var shaderSetsCount = reader.ReadInt32();
        for (var i = 0; i < shaderSetsCount; i++)
        {
            var backend = (GraphicsBackend)reader.ReadByte();
            var shadersLength = reader.ReadUInt32();
            if (backend == graphicsDevice.BackendType)
            {
                var shadersCount = reader.ReadInt32();
                shaderDescriptions = new ShaderDescription[shadersCount];
                for (var j = 0; j < shadersCount; j++)
                {
                    shaderDescriptions[j] = new ShaderDescription
                    {
                        Stage = (ShaderStages)reader.ReadByte(),
                        ShaderBytes = reader.ReadBytes(reader.ReadInt32()),
                        EntryPoint = reader.ReadString(),
                        Debug = reader.ReadBoolean(),
                    };
                }
            }
            else
            {
                reader.BaseStream.Position += shadersLength;
            }
        }

        var vertexElementsCount = reader.ReadInt32();
        var vertexElements = new VertexElementDescription[vertexElementsCount];
        for (var i = 0; i < vertexElementsCount; i++)
        {
            vertexElements[i] = new VertexElementDescription
            {
                Name = reader.ReadString(),
                Semantic = (VertexElementSemantic)reader.ReadByte(),
                Format = (VertexElementFormat)reader.ReadByte(),
                Offset = reader.ReadUInt32(),
            };
        }

        var resourceLayoutsCount = reader.ReadUInt32();
        ResourceLayouts = new ResourceLayout[resourceLayoutsCount];
        for (var i = 0; i < resourceLayoutsCount; i++)
        {
            var resourceLayoutDescription = new ResourceLayoutDescription();

            var resourceLayoutElementsCount = reader.ReadInt32();
            resourceLayoutDescription.Elements = new ResourceLayoutElementDescription[resourceLayoutElementsCount];
            for (var j = 0; j < resourceLayoutElementsCount; j++)
            {
                resourceLayoutDescription.Elements[j] = new ResourceLayoutElementDescription
                {
                    Name = reader.ReadString(),
                    Kind = (ResourceKind)reader.ReadByte(),
                    Stages = (ShaderStages)reader.ReadByte(),
                    Options = (ResourceLayoutElementOptions)reader.ReadInt32(),
                };
            }

            ResourceLayouts[i] = graphicsDevice.ResourceFactory.CreateResourceLayout(ref resourceLayoutDescription);
        }

        var vertexLayouts = new VertexLayoutDescription[1];
        vertexLayouts[0] = new VertexLayoutDescription(vertexElements);

        var vsShader = graphicsDevice.ResourceFactory.CreateShader(ref shaderDescriptions[0]);
        var fsShader = graphicsDevice.ResourceFactory.CreateShader(ref shaderDescriptions[1]);

        ShaderSetDescription = new ShaderSetDescription(
            vertexLayouts,
            new[]
            {
                vsShader,
                fsShader,
            });

        var blendStateDescription = new BlendStateDescription();

        blendStateDescription.BlendFactor = new RgbaFloat(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());

        blendStateDescription.AttachmentStates = new BlendAttachmentDescription[reader.ReadInt32()];
        for (var i = 0; i < blendStateDescription.AttachmentStates.Length; i++)
        {
            ref var attachmentState = ref blendStateDescription.AttachmentStates[i];

            attachmentState.BlendEnabled = reader.ReadBoolean();
            attachmentState.SourceColorFactor = (BlendFactor)reader.ReadByte();
            attachmentState.DestinationColorFactor = (BlendFactor)reader.ReadByte();
            attachmentState.ColorFunction = (BlendFunction)reader.ReadByte();
            attachmentState.SourceAlphaFactor = (BlendFactor)reader.ReadByte();
            attachmentState.DestinationAlphaFactor = (BlendFactor)reader.ReadByte();
            attachmentState.AlphaFunction = (BlendFunction)reader.ReadByte();
        }

        BlendStateDescription = blendStateDescription;

        DepthStencilStateDescription = new DepthStencilStateDescription
        {
            DepthTestEnabled = reader.ReadBoolean(),
            DepthWriteEnabled = reader.ReadBoolean(),
            DepthComparison = (ComparisonKind)reader.ReadByte(),
            StencilTestEnabled = reader.ReadBoolean(),
            StencilFront = reader.ReadStencilBehaviorDescription(),
            StencilBack = reader.ReadStencilBehaviorDescription(),
            StencilReadMask = reader.ReadByte(),
            StencilWriteMask = reader.ReadByte(),
            StencilReference = reader.ReadUInt32()
        };

        RasterizerStateDescription = new RasterizerStateDescription
        {
            CullMode = (FaceCullMode)reader.ReadByte(),
            FillMode = (PolygonFillMode)reader.ReadByte(),
            FrontFace = (FrontFace)reader.ReadByte(),
            DepthClipEnabled = reader.ReadBoolean(),
            ScissorTestEnabled = reader.ReadBoolean()
        };
    }

    public Pipeline GetPipeline(in OutputDescription outputDescription)
    {
        if (_pipeline != null)
        {
            if (!_pipelineOutDescription.Equals(outputDescription))
            {
                throw new InvalidOperationException();
            }
        }

        _pipelineOutDescription = outputDescription;

        _pipeline = _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(
                BlendStateDescription,
                DepthStencilStateDescription,
                RasterizerStateDescription,
                PrimitiveTopology.TriangleList,
                ShaderSetDescription,
                ResourceLayouts,
                outputDescription));

        return _pipeline;
    }

    public void Dispose()
    {
        if (_pipeline != null)
        {
            _pipeline.Dispose();
        }

        foreach (var resourceLayout in ResourceLayouts)
        {
            resourceLayout.Dispose();
        }

        foreach (var shader in ShaderSetDescription.Shaders)
        {
            shader.Dispose();
        }
    }
}

internal static class BinaryReaderExtensions
{
    public static StencilBehaviorDescription ReadStencilBehaviorDescription(this BinaryReader reader)
    {
        return new StencilBehaviorDescription
        {
            Fail = (StencilOperation)reader.ReadByte(),
            Pass = (StencilOperation)reader.ReadByte(),
            DepthFail = (StencilOperation)reader.ReadByte(),
            Comparison = (ComparisonKind)reader.ReadByte(),
        };
    }
}
