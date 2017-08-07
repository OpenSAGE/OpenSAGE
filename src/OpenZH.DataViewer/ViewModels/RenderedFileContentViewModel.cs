using OpenZH.Data;
using OpenZH.Graphics.LowLevel;

namespace OpenZH.DataViewer.ViewModels
{
    public abstract class RenderedFileContentViewModel : FileContentViewModel
    {
        public abstract bool RedrawsOnTimer { get; }

        public RenderedFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
        }

        public abstract void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain);

        public abstract void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain);
    }
}
