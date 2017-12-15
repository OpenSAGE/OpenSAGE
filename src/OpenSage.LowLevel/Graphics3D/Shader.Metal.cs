using System;
using Foundation;
using Metal;
using OpenSage.LowLevel.Graphics3D.Util;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class Shader
    {
        internal IMTLFunction DeviceFunction { get; private set; }

        internal override string PlatformGetDebugName() => DeviceFunction.GetLabel();
        internal override void PlatformSetDebugName(string value) => DeviceFunction.SetLabel(value);

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            string functionName,
            byte[] deviceBytecode, 
            out ShaderType shaderType,
            out ShaderResourceBinding[] resourceBindings)
        {
            using (var library = graphicsDevice.Device.CreateLibrary(NSData.FromArray(deviceBytecode), out _))
            {
                DeviceFunction = AddDisposable(library.CreateFunction(functionName));
            }

            shaderType = DeviceFunction.FunctionType.ToShaderType();

            throw new NotImplementedException("Reflection not yet implemented.");
        }
    }
}
