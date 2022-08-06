using System;
using System.Collections.Generic;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Core.Graphics;

public sealed class GraphicsDeviceManager : DisposableBase
{
    private readonly Dictionary<uint, DeviceBuffer> _nullStructuredBuffers;

    public readonly GraphicsDevice GraphicsDevice;

    public Sampler Aniso4xClampSampler { get; }
    public Sampler LinearClampSampler { get; }
    public Sampler PointClampSampler { get; }

    public readonly Dictionary<string, object> Data = new();

    public GraphicsDeviceManager(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;

        var aniso4xClampSamplerDescription = SamplerDescription.Aniso4x;
        aniso4xClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
        aniso4xClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
        aniso4xClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
        Aniso4xClampSampler = AddDisposable(
            graphicsDevice.ResourceFactory.CreateSampler(ref aniso4xClampSamplerDescription));
        Aniso4xClampSampler.Name = "Aniso4xClamp Sampler";

        var linearClampSamplerDescription = SamplerDescription.Linear;
        linearClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
        linearClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
        linearClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
        LinearClampSampler = AddDisposable(
            graphicsDevice.ResourceFactory.CreateSampler(ref linearClampSamplerDescription));
        LinearClampSampler.Name = "LinearClamp Sampler";

        var pointClampSamplerDescription = SamplerDescription.Point;
        pointClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
        pointClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
        pointClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
        PointClampSampler = AddDisposable(
            graphicsDevice.ResourceFactory.CreateSampler(ref pointClampSamplerDescription));
        PointClampSampler.Name = "PointClamp Sampler";

        _nullStructuredBuffers = new Dictionary<uint, DeviceBuffer>();
    }

    public DeviceBuffer GetNullStructuredBuffer(uint size)
    {
        if (!_nullStructuredBuffers.TryGetValue(size, out var result))
        {
            _nullStructuredBuffers.Add(size, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    size,
                    BufferUsage.StructuredBufferReadOnly,
                    size,
                    true))));

            result.Name = $"NullStructuredBuffer_Size{size}";
        }
        return result;
    }

    protected override void Dispose(bool disposeManagedResources)
    {
        base.Dispose(disposeManagedResources);

        foreach (var value in Data.Values)
        {
            if (value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
