using System;

namespace OpenSage.DataViewer.ViewModels
{
    public interface IGameViewModel : IDisposable
    {
        Game Game { get; }
    }
}
