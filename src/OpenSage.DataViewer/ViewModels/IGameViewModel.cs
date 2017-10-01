using System;

namespace OpenSage.DataViewer.ViewModels
{
    public interface IGameViewModel : IDisposable
    {
        Graphics.Cameras.Controllers.CameraController CameraController { get; }
        void OnMouseMove(int x, int y);

        Game Game { get; }
    }
}
