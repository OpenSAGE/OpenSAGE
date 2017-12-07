using System.Windows.Controls;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Elements;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class WndFileContentViewModel : FileContentViewModel, IGameViewModel
    {
        private readonly FileSystem _fileSystem;
        
        public Canvas ContainerView { get; }

        public WndFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _fileSystem = file.FileSystem;
        }

        void IGameViewModel.LoadScene(Game game)
        {
            var guiComponent = new GuiComponent
            {
                RootWindow = game.ContentManager.Load<UIElement>(File.FilePath)
            };

            var scene = new Scene();

            var entity = new Entity();
            entity.Components.Add(guiComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;
        }
    }
}
