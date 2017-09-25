using System;
using LLGfx;
using OpenSage.DataViewer.Framework;

namespace OpenSage.DataViewer.ViewModels
{
    public interface IRenderableViewModel : IDisposable
    {
        CameraController CameraController { get; }

        void Draw(
            GraphicsDevice graphicsDevice, 
            SwapChain swapChain, 
            RenderPassDescriptor renderPassDescriptor);

        void OnMouseMove(int x, int y);
    }
}
